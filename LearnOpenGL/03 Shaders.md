# Shaders 着色器

* 是运行在GPU上的小程序
* 只是一种把输入转化为输出的程序。
* 着色器之间不能相互通信；

## GLSL

类C语言

* 开头总是要声明版本
* 接着是输入和输出变量
* uniform
* main函数，每个着色器的入口点都是main函数

一个典型的着色器有下面的结构：

```c++
#version version_number
in type in_variable_name;
in type in_variable_name;

out type out_variable_name;

uniform type uniform_name;

int main()
{
  // 处理输入并进行一些图形操作
  ...
  // 输出处理过的结果到输出变量
  out_variable_name = weird_stuff_we_processed;
}
```

### 顶点属性(Vertex Attribute)

顶点属性是有上限的，它一般由硬件来决定

* OpenGL确保至少有16个包含4分量的顶点属性可用

* 你可以查询**GL_MAX_VERTEX_ATTRIBS**来获取具体的上限：

  ```c++
  int nrAttributes;
  glGetIntegerv(GL_MAX_VERTEX_ATTRIBS, &nrAttributes);
  std::cout << "Maximum nr of vertex attributes supported: " << nrAttributes << std::endl;
  ```

## 数据类型

包含C等其它语言大部分的默认基础数据类型：

* `int`、`float`、`double`、`uint`和`bool`。

也有两种容器类型:

* 向量(Vector)和矩阵(Matrix)

### 向量

**GLSL**中的向量是一个可以包含有2、3或者4个分量的容器，分量的类型可以是前面默认基础类型的任意一个。它们可以是下面的形式（`n`代表分量的数量）：

| 类型    | 含义                            |
| :------ | :------------------------------ |
| `vecn`  | 包含`n`个float分量的默认向量    |
| `bvecn` | 包含`n`个bool分量的向量         |
| `ivecn` | 包含`n`个int分量的向量          |
| `uvecn` | 包含`n`个unsigned int分量的向量 |
| `dvecn` | 包含`n`个double分量的向量       |

大多数时候我们使用`vecn`,，因为float足够满足大多数要求了。

### 分量

一个向量的分量可以通过`vec.x`这种方式获取，这里`x`是指这个向量的第一个分量。

* 你可以分别使用`.x`、`.y`、`.z`和`.w`来获取它们的第1、2、3、4个分量。

* GLSL也允许你对颜色使用`rgba`
* 或是对纹理坐标使用`stpq`访问相同的分量。

### 重组（Swizzing）

向量这一数据类型也允许一些有趣而灵活的分量选择方式，叫做**重组(Swizzling)**。重组允许这样的语法：

```c++
vec2 someVec;
vec4 differentVec = someVec.xyxx;
vec3 anotherVec = differentVec.zyw;
vec4 otherVec = someVec.xxxx + anotherVec.yxzy;
```

你可以使用上面4个字母任意组合来创建一个和原来向量一样长的（同类型）新向量，只要原来向量有那些分量即可；

---

我们也可以把一个向量作为一个参数传给不同的向量构造函数，以减少需求参数的数量：

```c++
vec2 vect = vec2(0.5, 0.7);
vec4 result = vec4(vect, 0.0, 0.0);
vec4 otherResult = vec4(result.xyz, 1.0);
```

## 输入与输出

GLSL定义了`in`和`out`关键字专门来实现输入与输出

* 只要一个输出变量与下一个着色器阶段的输入匹配，它就会传递下去。

* 但在顶点和片段着色器中会有点不同:

  * 顶点着色器应该接收的是一种特殊形式(**从顶点数据中直接接收输入**)的输入，否则就会效率低下。

    * 为了定义顶点数据该如何管理，我们使用`location`这一元数据指定输入变量，这样我们才可以在CPU上配置顶点属性。

    > 也可以忽略`layout (location = 0)`标识符，通过在**OpenGL**代码中使用`glGetAttribLocation`查询属性位置值(Location)，
    >
    > 
    >
    > 但是在着色器中设置它们，这样会更容易理解而且节省自己（和OpenGL）工作量。

  * 片段着色器，因为需要生成一个最终输出的颜色，它需要一个`vec4`颜色输出变量。

    * 如果没有定义，则会把你的物体渲染成黑色或白色

* 如果我们打算从一个着色器向另一个着色器发送数据，我们必须在发送方着色器中声明一个输出，在接收方着色器中声明一个类似的输入。
  * 当**类型和名字都一样**的时候，OpenGL就会把两个变量链接到一起，它们之间就能发送数据

**顶点着色器**

```c++
#version 330 core
layout (location = 0) in vec3 aPos; // 位置变量的属性位置值为0

out vec4 vertexColor; // 为片段着色器指定一个颜色输出

void main()
{
    gl_Position = vec4(aPos, 1.0); // 注意我们如何把一个vec3作为vec4的构造器的参数
    vertexColor = vec4(0.5, 0.0, 0.0, 1.0); // 把输出变量设置为暗红色
}
```

**片段着色器**

```c++
#version 330 core
out vec4 FragColor;

in vec4 vertexColor; // 从顶点着色器传来的输入变量（名称相同、类型相同）

void main()
{
    FragColor = vertexColor;
}
```

## Uniform

是一种从CPU中的应用向GPU中的着色器发送数据的方式，但uniform和顶点属性有些不同。

* 首先，**uniform**是全局的(Global)。
  * 意味着uniform变量必须在每个着色器程序对象中都是独一无二的
  * 可以被着色器程序的任意着色器在任意阶段访问
* 无论你把uniform值设置成什么，uniform会一直保存它们的数据，直到它们被重置或更新。

```c++
#version 330 core
out vec4 FragColor;

uniform vec4 ourColor; // 在OpenGL程序代码中设定这个变量

void main()
{
    FragColor = ourColor;
}
```

因为uniform是全局变量，我们可以在**任何着色器**中定义它们，而无需通过顶点着色器作为中介。

顶点着色器中不需要这个uniform，所以我们不用在那里定义它

> 如果你声明了一个uniform却在GLSL代码中**没用过**，编译器会**静默移除**这个变量，导致最后编译出的版本中并不会包含它，这可能导致几个非常麻烦的错误，记住这点！

### 更新

这个uniform现在还是空的，我们首先需要找到着色器中uniform属性的**索引/位置值**。当我们得到uniform的索引/位置值后，我们就可以更新它的值

让它随着时间改变颜色：

```c++
float timeValue = glfwGetTime();//获取运行的秒数
float greenValue = (sin(timeValue) / 2.0f) + 0.5f;//颜色随时间变化
int vertexColorLocation = glGetUniformLocation(shaderProgram, "ourColor");//查询位置值
glUseProgram(shaderProgram);//必须先使用程序
glUniform4f(vertexColorLocation, 0.0f, greenValue, 0.0f, 1.0f);//设置uniform值
```

---

因为OpenGL在其核心是一个C库，所以它**不支持类型重载**，在函数参数不同的时候就要为其定义新的函数；glUniform是一个典型例子。这个函数有一个特定的后缀，标识设定的uniform的类型。

可能的后缀有：

| 后缀 | 含义                                 |
| :--- | :----------------------------------- |
| `f`  | 函数需要一个float作为它的值          |
| `i`  | 函数需要一个int作为它的值            |
| `ui` | 函数需要一个unsigned int作为它的值   |
| `3f` | 函数需要3个float作为它的值           |
| `fv` | 函数需要一个float向量/数组作为它的值 |

每当你打算配置一个OpenGL的选项时就可以简单地根据这些规则选择适合你的数据类型的重载函数。

在我们的例子里，我们希望分别设定uniform的4个float值，所以我们通过`glUniform4f`传递我们的数据(注意，我们也可以使用`fv`版本)。

---

### 渲染

下面我们就计算greenValue然后每个渲染迭代都更新这个uniform

```
while(!glfwWindowShouldClose(window))
{
    // 输入
    processInput(window);

    // 渲染
    // 清除颜色缓冲
    glClearColor(0.2f, 0.3f, 0.3f, 1.0f);
    glClear(GL_COLOR_BUFFER_BIT);

    // 记得激活着色器
    glUseProgram(shaderProgram);

    // 更新uniform颜色
    float timeValue = glfwGetTime();
    float greenValue = sin(timeValue) / 2.0f + 0.5f;
    int vertexColorLocation = glGetUniformLocation(shaderProgram, "ourColor");
    glUniform4f(vertexColorLocation, 0.0f, greenValue, 0.0f, 1.0f);

    // 绘制三角形
    glBindVertexArray(VAO);
    glDrawArrays(GL_TRIANGLES, 0, 3);

    // 交换缓冲并查询IO事件
    glfwSwapBuffers(window);
    glfwPollEvents();
}
```

## 更多属性！

这次，我们同样打算把颜色数据加进顶点数据中。我们将把颜色数据添加为3个float值至vertices数组。

我们将把三角形的三个角分别指定为红色、绿色和蓝色：

```c++
float vertices[] = {
    // 位置              // 颜色
     0.5f, -0.5f, 0.0f,  1.0f, 0.0f, 0.0f,   // 右下
    -0.5f, -0.5f, 0.0f,  0.0f, 1.0f, 0.0f,   // 左下
     0.0f,  0.5f, 0.0f,  0.0f, 0.0f, 1.0f    // 顶部
};
```

### 着色器

需要注意的是我们用`layout`标识符来把**顶点着色器**中的aColor属性的位置值设置为1：

```c++
#version 330 core
layout (location = 0) in vec3 aPos;   // 位置变量的属性位置值为 0 
layout (location = 1) in vec3 aColor; // 颜色变量的属性位置值为 1

out vec3 ourColor; // 向片段着色器输出一个颜色

void main()
{
    gl_Position = vec4(aPos, 1.0);
    ourColor = aColor; // 将ourColor设置为我们从顶点数据那里得到的输入颜色
}
```

由于我们不再使用uniform来传递片段的颜色了，现在使用`ourColor`输出变量，我们必须再修改一下片段着色器：

```c++
#version 330 core
out vec4 FragColor;  
in vec3 ourColor;

void main()
{
    FragColor = vec4(ourColor, 1.0);
}
```

### 顶点属性

因为我们添加了另一个顶点属性，并且更新了VBO的内存，我们就必须重新配置顶点属性指针。更新后的VBO内存中的数据现在看起来像这样：

![img](D:\Program\OpenGL\LearnOpenGL\03 Shaders.assets\vertex_attribute_pointer_interleaved.png)

知道了现在使用的布局，我们就可以使用glVertexAttribPointer函数更新顶点格式，

```c++
// 位置属性
glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 6 * sizeof(float), (void*)0);
glEnableVertexAttribArray(0);
// 颜色属性
glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, 6 * sizeof(float), (void*)(3* sizeof(float)));
glEnableVertexAttribArray(1);
```

### 步长值

由于我们现在有了两个顶点属性，我们不得不重新计算**步长**值。为获得数据队列中下一个属性值（比如位置向量的下个`x`分量）我们必须向右移动6个float，其中3个是位置值，另外3个是颜色值。这使我们的步长值为6乘以float的字节数（=24字节）。

### 偏移量（OFFSET）

同样，这次我们必须指定一个偏移量。对于每个顶点来说，位置顶点属性在前，所以它的偏移量是0。颜色属性紧随位置数据之后，所以偏移量就是`3 * sizeof(float)`，用字节来计算就是12字节。

# 我们自己的着色器类

可以从硬盘读取着色器，然后编译并链接它们，并对它们进行错误检测

我们会把着色器类全部放在在头文件里，主要是为了学习用途，当然也方便移植。我们先来添加必要的include，并定义类结构：

```c++
#ifndef SHADER_H
#define SHADER_H

#include <glad/glad.h>; // 包含glad来获取所有的必须OpenGL头文件

#include <string>
#include <fstream>
#include <sstream>
#include <iostream>


class Shader
{
public:
    // 程序ID
    unsigned int ID;

    // 构造器读取并构建着色器
    Shader(const char* vertexPath, const char* fragmentPath);
    // 使用/激活程序
    void use();
    // uniform工具函数
    void setBool(const std::string &name, bool value) const;  
    void setInt(const std::string &name, int value) const;   
    void setFloat(const std::string &name, float value) const;
};

#endif
```

> 在上面，我们在头文件顶部使用了几个预处理指令(Preprocessor Directives)。这些预处理指令会告知你的编译器只在它没被包含过的情况下才包含和编译这个头文件，即使多个文件都包含了这个着色器头文件。它是用来防止链接冲突的。