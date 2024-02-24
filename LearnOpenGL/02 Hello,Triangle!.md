# 在学习此节之前，建议将这三个单词先记下来：

- 顶点数组对象：Vertex Array Object，VAO
- 顶点缓冲对象：Vertex Buffer Object，VBO
- 元素缓冲对象：Element Buffer Object，EBO 或 索引缓冲对象 Index Buffer Object，IBO



# 图形渲染管线 Graphics Pipeline

* 3D坐标转为2D坐标的处理过程

# 顶点输入

## 顶点

* 一个float数组

```glsl
float vertices[] = {
    -0.5f, -0.5f, 0.0f,
     0.5f, -0.5f, 0.0f,
     0.0f,  0.5f, 0.0f
};
```

## 深度

* 由于OpenGL是在3D空间中工作的，而我们渲染的是一个2D三角形，我们将它顶点的z坐标设置为0.0。这样子的话三角形每一点的***深度*(Depth，译注2)**都是一样的，从而使它看上去像是2D的。
* 通常深度可以理解为**z坐标**，它代表一个像素在空间中和你的距离，如果离你远就可能被别的像素遮挡，你就看不到它了，它会被丢弃，以节省资源。

## 顶点缓冲对象(Vertex Buffer Objects, VBO)

它会在GPU内存（通常被称为显存）中储存大量顶点。使用这些缓冲对象的好处是我们可以一次性的发送一大批数据到显卡上，而不是每个顶点发送一次。

* 这个缓冲有一个**独一无二的ID**

* 使用`glGenBuffers`函数和一个缓冲`ID`生成一个`VBO`对象

  ```
  unsigned int VBO;
  glGenBuffers(1, &VBO);
  ```

* 顶点缓冲对象的缓冲类型是`GL_ARRAY_BUFFER`

* OpenGL允许**同时绑定**多个缓冲，只要它们是**不同的**缓冲类型

* 使用**glBindBuffer**函数把新创建的缓冲绑定到**GL_ARRAY_BUFFER**目标上

  ```
  glBindBuffer(GL_ARRAY_BUFFER, VBO);  
  ```

* 我们使用的任何（在GL_ARRAY_BUFFER目标上的）缓冲调用都会用来配置当前绑定的缓冲(VBO)。

* 我们可以调用`glBufferData`函数，它会把之前定义的顶点数据**复制**到缓冲的内存中：

  * **第一个参数是目标缓冲的类型**:	顶点缓冲对象当前绑定到GL_ARRAY_BUFFER目标上
  * **第二个参数指定传输数据的大小**(以字节为单位):sizeof即可
  * **第三个参数是我们希望发送的实际数据**:vertices顶点数组
  * **第四个参数指定了我们希望显卡如何管理给定的数据**：有三种形式
    * GL_STATIC_DRAW ：数据不会或几乎不会改变：
    * GL_DYNAMIC_DRAW：数据会被改变很多。
    * GL_STREAM_DRAW ：数据每次绘制时都会改变。
    * 三角形的位置数据不会改变，每次渲染调用时都保持原样，所以它的使用类型最好是**GL_STATIC_DRAW**。

  ```
  glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);
  ```

  现在我们已经把顶点数据储存在显卡的内存中，用VBO这个顶点缓冲对象管理。

# 着色器

* 大多数显卡都有成千上万的小处理核心，它们在**GPU上**为每一个（渲染管线）阶段运行各自的**小程序**，从而在图形渲染管线中快速处理你的数据。这些小程序叫做**着色器(Shader)。**

* OpenGL着色器语言(OpenGL Shading Language, **GLSL**)

* 我们是希望把这些数据渲染成一系列的点？一系列的三角形？还是仅仅是一个长长的线？做出的这些提示叫做**图元(Primitive)**，任何一个绘制指令的调用都将把图元传递给OpenGL。
* 经顶点着色器处理过，即是**标准化设备坐标**(Normalized Device Coordinates），显示在屏幕上

## 第一阶段 顶点着色器(Vertex Shader)

* 单独顶点
* 转换空间坐标到屏幕空间

* 允许对顶点属性进行一些基本处理。

### 着色器的书写

#### 声明版本号、核心模式

`#version 330 core`

#### `in` 关键字

* 声明所有的**输入顶点属性(Input Vertex Attribute)**

#### GLSL向量数据类型

* 包含1到4个`float`分量，前三个对于aPos输入的三个坐标分量

### 位置值

* `layout (location = 0)`

```c++
#version 330 core
layout (location = 0) in vec3 aPos;

void main()
{
    gl_Position = vec4(aPos.x, aPos.y, aPos.z, 1.0);//vec3转换程vec4,vec4.w=1.0f
}
```



## * **向量(Vector)**

> 在GLSL中一个向量有最多4个分量，每个分量值都代表空间中的一个坐标，它们可以通过`vec.x`、`vec.y`、`vec.z`和`vec.w`来获取。注意`vec.w`分量不是用作表达空间中的位置的（我们处理的是3D不是4D），而是用在所谓**透视除法**(Perspective Division)上

## 编译着色器

### 代码

* 暂时将顶点着色器的源代码硬编码在代码文件顶部的C风格字符串

  ```glsl
  const char *vertexShaderSource = "#version 330 core\n"
      "layout (location = 0) in vec3 aPos;\n"
      "void main()\n"
      "{\n"
      "   gl_Position = vec4(aPos.x, aPos.y, aPos.z, 1.0);\n"
      "}\0";
  ```

* 为了能够让OpenGL使用它，我们必须在运行时动态编译它的源代码

### 创建着色器对象

* 创建一个着色器对象，用ID来引用的，储存这个顶点着色器为`unsigned int`。然后用**glCreateShader**创建这个着色器：

> 我们把需要创建的着色器类型以**参数形式**提供给`glCreateShader`。由于我们正在创建一个顶点着色器，传递的参数是`GL_VERTEX_SHADER`。

```glsl
unsigned int vertexShader;
vertexShader = glCreateShader(GL_VERTEX_SHADER);
```

* 把这个着色器源码附加到着色器对象上编译

* **glShaderSource**函数
  * 把要**编译的着色器对象作为第一个参数**。
  * **第二参数指定了传递的源码字符串数量**，这里只有一个。
  * **第三个参数是顶点着色器真正的源码**
  * 第四个参数我们先设置为`NULL`。

```
glShaderSource(vertexShader, 1, &vertexShaderSource, NULL);
glCompileShader(vertexShader);
```

### * Debug检测shader是否编译成功

你可能会希望检测在调用glCompileShader后编译是否成功了，如果没成功的话，你还会希望知道错误是什么，这样你才能修复它们。检测编译时错误可以通过以下代码来实现：

```
int  success;
char infoLog[512];
glGetShaderiv(vertexShader, GL_COMPILE_STATUS, &success);
```

首先我们定义一个整型变量来表示是否成功编译，还定义了一个储存错误消息（如果有的话）的容器。然后我们用glGetShaderiv检查是否编译成功。如果编译失败，我们会用glGetShaderInfoLog获取错误消息，然后打印它。

```
if(!success)
{
    glGetShaderInfoLog(vertexShader, 512, NULL, infoLog);
    std::cout << "ERROR::SHADER::VERTEX::COMPILATION_FAILED\n" << infoLog << std::endl;
}
```

## 图元装配(Primitive Assembly)阶段

* 所有顶点作为输入
* 装配成图元指定的形状

### 几何着色器

* 顶点的集合作为输入
* 可以通过产生**新顶点**构造出新的（或是其它的）图元来生成其他形状

### 光栅化阶段

* 图元映射为最终屏幕上相应的**像素**
* 生成供**片段着色器(Fragment Shader)**使用的**片段(Fragment)**

#### 裁切

* **丢弃**超出你的**视图以外**的所有像素

## 片段着色器

### 主要目的

* 计算一个像素的最终颜色，所有OpenGL高级效果产生的地方

### *RGBA

RGB可生成超1600种颜色！！！

### 着色器程序

#### `out`关键字

* 表示的是最终的输出颜色，我们应该自己将其计算出来

* 声明输出变量可以使用`out`关键字，这里我们命名为FragColor

  ```
  #version 330 core
  out vec4 FragColor;
  
  void main()
  {
      FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);
  } 
  ```

### 编译片段着色器

与顶点着色器类似，只不过我们使用GL_FRAGMENT_SHADER常量作为着色器类型

```
unsigned int fragmentShader;
fragmentShader = glCreateShader(GL_FRAGMENT_SHADER);
glShaderSource(fragmentShader, 1, &fragmentShaderSource, NULL);
glCompileShader(fragmentShader);
```

## Alpha测试和混合（Blending）阶段

* 检测深度值，决定是否丢弃
* 检测Alpha值和对物体进行混合
  

## 着色器程序

* 着色器程序对象(Shader Program Object)是多个着色器合并之后并最终链接完成的版本
* 要使用刚才编译的着色器我们必须把它们链接(Link)为**一个**着色器程序对象
* 在渲染对象的时候激活这个着色器程序

### 链接

* 创建一个程序对象

```
unsigned int shaderProgram;
shaderProgram = glCreateProgram();
```

* **glCreateProgram**函数创建一个程序，并返回新创建程序对象的ID引用。

* 现在我们需要把之前编译的着色器附加到程序对象上，然后**用glLinkProgram链接**它们

  ```d
  glAttachShader(shaderProgram, vertexShader);
  glAttachShader(shaderProgram, fragmentShader);
  glLinkProgram(shaderProgram);
  ```

#### Debug链接着色器

* 不会调用**glGetShaderiv**和**glGetShaderInfoLog**,，现在我们使用：

```
glGetProgramiv(shaderProgram, GL_LINK_STATUS, &success);
if(!success) {
    glGetProgramInfoLog(shaderProgram, 512, NULL, infoLog);
    ...
}
```

#### 激活与删除

* 激活

  ```
  glUseProgram(shaderProgram);
  ```

* 链接完毕后，就可以删除着色器对象了

  ```
  glDeleteShader(vertexShader);
  glDeleteShader(fragmentShader);
  ```

  

## 链接顶点属性

* 顶点着色器允许我们指定任何以顶点属性为形式的输入。
* 具有很强的灵活性的同时，它还的确意味着我们必须**手动指定**输入数据的哪一个部分对应顶点着色器的哪一个顶点属性。
* 我们的顶点缓冲数据会被解析为下面这![img](https://learnopengl-cn.github.io/img/01/04/vertex_attribute_pointer.png)样子：
  * 位置数据被储存为**32位（4字节）浮点值**。
  * 每个位置包含**3个**这样的值。
  * 在这**3个值之间没有空隙**（或其他值）。这几个值在数组中**紧密排列(Tightly Packed)**。
  * 数据中**第一个值在缓冲开始的位置**。

* 使用**glVertexAttribPointer**函数告诉OpenGL该如何解析顶点数据（应用到逐个顶点属性上）了：
  * **第一个参数指定我们要配置的顶点属性**:我们在**顶点着色器**使用`layout(location = 0)`定义了position顶点属性的**位置值(Location)**吗？它可以把顶点属性的位置值设置为`0`。因为我们希望把数据传递到这一个顶点属性中，所以这里我们传入`0`。
    * 每个顶点属性从一个VBO管理的内存中获得它的数据，而具体是从哪个VBO（程序中可以有多个VBO）获取则是通过在调用glVertexAttribPointer时绑定到GL_ARRAY_BUFFER的VBO决定的。
    * 由于在调用glVertexAttribPointer之前绑定的是先前定义的VBO对象，**顶点属性`0`**现在会链接到它的顶点数据。
  * **第二个参数指定顶点属性的大小**：顶点属性是一个`vec3`，它由3个值组成，所以大小是3
  * **第三个参数指定数据的类型**，这里是GL_FLOAT(GLSL中`vec*`都是由浮点数值组成的)。
  * **第四个参数定义我们是否希望数据被标准化(Normalize)**。如果我们设置为GL_TRUE，所有数据都会被**映射到0（对于有符号型signed数据是-1）到1**之间。我们把它设置为GL_FALSE。
  * **第五个参数叫做步长(Stride)，它告诉我们在连续的顶点属性组之间的间隔**。
    * 由于下个组位置数据在3个`float`之后，我们把步长设置为`3 * sizeof(float)`。
    * 由于我们知道这个数组是紧密排列的（在两个顶点属性之间没有空隙）我们也可以**设置为0**来让OpenGL决定具体步长是多少（**只有当数值是紧密排列时才可用**）。
    * 一旦我们有更多的顶点属性，我们就必须更小心地定义每个顶点属性之间的间隔
  * **最后一个参数表示位置数据在缓冲中起始位置的偏移量(Offset)**。
    * 由于类型是`void*`，所以需要我们进行这个奇怪的强制类型转换。
    * 由于位置数据在数组的开头，所以这里是0。我们会在后面详细解释这个参数
* 使用**glEnableVertexAttribArray**，以**顶点属性位置值**作为参数，启用顶点属性；
  * 顶点属性默认是禁用的。

```cpp
glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 3 * sizeof(float), (void*)0);
glEnableVertexAttribArray(0);//默认禁用
```

### 总结

我们使用一个顶点缓冲对象将顶点数据初始化至缓冲中，建立了一个顶点和一个片段着色器，并告诉了OpenGL如何把顶点数据链接到顶点着色器的顶点属性上。

在OpenGL中绘制一个物体，代码会像是这样：

```cpp
// 0. 复制顶点数组到缓冲中供OpenGL使用
glBindBuffer(GL_ARRAY_BUFFER, VBO);
glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);
// 1. 设置顶点属性指针
glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 3 * sizeof(float), (void*)0);
glEnableVertexAttribArray(0);
// 2. 当我们渲染一个物体时要使用着色器程序
glUseProgram(shaderProgram);
// 3. 绘制物体
someOpenGLFunctionThatDrawsOurTriangle();
```

* 每当我们绘制一个物体的时候都必须重复这一过程
* 如果有超过5个顶点属性，上百个不同物体呢（这其实并不罕见）。绑定正确的缓冲对象，为每个物体配置所有顶点属性很快就变成一件麻烦事

### 顶点数组对象(Vertex Array Object, VAO)

* 可以像顶点缓冲对象那样被绑定，任何随后的顶点属性调用都会储存在这个VAO中。
* 当配置顶点属性指针时，你只需要将那些调用执行一次，之后再绘制物体的时候只需要绑定相应的VAO就行了。
* 这使在不同顶点数据和属性配置之间切换变得非常简单，只需要绑定不同的VAO就行了。

OpenGL的核心模式**要求**我们使用VAO，所以它知道该如何处理我们的顶点输入。如果我们绑定VAO失败，OpenGL会拒绝绘制任何东西。

* 一个顶点数组对象会储存以下这些内容：
  * **glEnableVertexAttribArray**和**glDisableVertexAttribArray**的调用。
  * 通过**glVertexAttribPointer**设置的**顶点属性配置**。
  * 通过**glVertexAttribPointer**调用与**顶点属性关联的顶点缓冲对象**。

![img](D:\Program\OpenGL\LearnOpenGL\02 Hello,Triangle!.assets\vertex_array_objects.png)

* 创建一个VAO和创建一个VBO很类似：

  ```CPP
  unsigned int VAO;
  glGenVertexArrays(1, &VAO);
  ```

* 用VAO，要做的只是使用glBindVertexArray绑定VAO

* 从绑定之后起，我们应该绑定和配置对应的VBO和属性指针，之后解绑VAO供之后使用。

  * 当我们打算绘制一个物体的时候，我们只要在绘制物体前简单地把VAO绑定到希望使用的设定上就行了。这段代码应该看起来像这样：

```
// ..:: 初始化代码（只运行一次 (除非你的物体频繁改变)） :: ..
// 1. 绑定VAO
glBindVertexArray(VAO);
// 2. 把顶点数组复制到缓冲中供OpenGL使用
glBindBuffer(GL_ARRAY_BUFFER, VBO);
glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);
// 3. 设置顶点属性指针
glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 3 * sizeof(float), (void*)0);
glEnableVertexAttribArray(0);

[...]

// ..:: 绘制代码（渲染循环中） :: ..
// 4. 绘制物体
glUseProgram(shaderProgram);
glBindVertexArray(VAO);
someOpenGLFunctionThatDrawsOurTriangle();
```

一般当你打算绘制多个物体时，你首先要生成/配置所有的VAO（和必须的VBO及属性指针)，然后储存它们供后面使用。当我们打算绘制物体的时候就拿出相应的VAO，绑定它，绘制完物体后，再解绑VAO。

### 我们一直期待的三角形

要想绘制我们想要的物体，OpenGL给我们提供了**glDrawArrays**函数，它使用当前激活的着色器，之前定义的顶点属性配置，和VBO的顶点数据（通过VAO间接绑定）来绘制图元。

#### glDrawArrays

* 第一个参数希望绘制的是一个三角形，这里传递GL_TRIANGLES给它
* 第二个参数指定了顶点数组的起始索引,我们这里填`0`
* 最后一个参数指定我们打算绘制多少个顶点，这里是`3`

```CPP
glUseProgram(shaderProgram);
glBindVertexArray(VAO);
glDrawArrays(GL_TRIANGLES, 0, 3);
```

#### 成果

![image-20230722012119443](D:\Program\OpenGL\LearnOpenGL\02 Hello,Triangle!.assets\image-20230722012119443.png)

## 元素（索引）缓冲对象 Element（Index） Buffer Object，EBO

我们可以绘制两个三角形来组成一个矩形（OpenGL主要处理三角形），而六个顶点有可能叠加，就会产生多的开销：

* 我们设定绘制顺序，只用存储4个顶点
*  EBO是一个缓冲区，就像一个顶点缓冲区对象一样，它存储 OpenGL 用来决定要绘制哪些顶点的索引。

首先，我们先要定义（不重复的）顶点，和绘制出矩形所需的索引：

```
float vertices[] = {
    0.5f, 0.5f, 0.0f,   // 右上角
    0.5f, -0.5f, 0.0f,  // 右下角
    -0.5f, -0.5f, 0.0f, // 左下角
    -0.5f, 0.5f, 0.0f   // 左上角
};

unsigned int indices[] = {
    // 注意索引从0开始! 
    // 此例的索引(0,1,2,3)就是顶点数组vertices的下标，
    // 这样可以由下标代表顶点组合成矩形

    0, 1, 3, // 第一个三角形
    1, 2, 3  // 第二个三角形
};
```

下一步我们需要创建元素缓冲对象：

```c++
unsigned int EBO;
glGenBuffers(1, &EBO);
```

类似于VBO，先绑定EBO，再把索引复制到缓冲里，不过这次我们缓冲定义为**GL_ELEMENT_ARRAY_BUFFER**

```c++
glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, EBO);
glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(indices), indices, GL_STATIC_DRAW);
```

最后一件要做的事是用glDrawElements来替换glDrawArrays函数，表示我们要从索引缓冲区渲染三角形。

* **第一个参数指定了我们绘制的模式**，这个和glDrawArrays的一样。
* **第二个参数是我们打算绘制顶点的个数**，这里填6，
* **第三个参数是索引的类型**，这里是GL_UNSIGNED_INT
* **最后一个参数里我们可以指定EBO中的偏移量**

```
glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, EBO);
glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, 0);
```

