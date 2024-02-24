# 了解一下OpenGL

> OpenGL本身**并不是一个API**，它仅仅是一个由[Khronos组织](http://www.khronos.org/)制定并维护的**规范(Specification)**。

* 开发者通常是显卡的生产商

## 核心模式与立即渲染模式

### 立即渲染模式

* 立即渲染模式确实容易使用和理解，但是效率太低。

* OpenGL3.2开始，规范文档开始废弃立即渲染模式

### 核心模式 3.3以后

* 使用OpenGL的**核心模式**时，OpenGL迫使我们使用**现代的函数**

* 现代函数的优势是更高的**灵活性和效率**，然而也更难于学习。

* 当使用新版本的OpenGL特性时，只有新一代的显卡能够支持你的应用程序。

## 扩展

* OpenGL的一大**特性**就是**对扩展(Extension)的支持**

* 当一个显卡公司提出一个新特性或者渲染上的大优化，通常会以扩展的方式在驱动中实现
* 开发者不必等待一个新的OpenGL规范面世，就可以使用这些新的渲染特性

使用规范：

```glsl
if(GL_ARB_extension_name)
{
    // 使用硬件支持的全新的现代特性
}
else
{
    // 不支持此扩展: 用旧的方式去做
}
```

## 状态机

* OpenGL自身是一个巨大的**状态机(State Machine)**

* OpenGL的状态通常被称为**OpenGL上下文(Context)**

* 我们会遇到一些**状态设置函数(State-changing Function)**，这类函数将会改变上下文。

### 对象

* 在OpenGL中一个对象是指一些选项的集合

* 可以把对象看做一个C风格的结构体(Struct)：

``` glsl
struct object_name {
    float  option1;
    int    option2;
    char[] name;
};
```

* 当我们使用一个对象时，通常看起来像如下一样（把OpenGL上下文看作一个大的结构体）：

````
// OpenGL的状态
struct OpenGL_Context {
    ...
    object* object_Window_Target;
    ...     
};

````

```glsl
// 创建对象
unsigned int objectId = 0;
glGenObject(1, &objectId);
// 绑定对象至上下文
glBindObject(GL_WINDOW_TARGET, objectId);
// 设置当前绑定到 GL_WINDOW_TARGET 的对象的一些选项
glSetObjectOption(GL_WINDOW_TARGET, GL_OPTION_WINDOW_WIDTH, 800);
glSetObjectOption(GL_WINDOW_TARGET, GL_OPTION_WINDOW_HEIGHT, 600);
// 将上下文对象设回默认
glBindObject(GL_WINDOW_TARGET, 0);
```

* 使用对象的一个好处是在程序中，我们**不止可以定义一个对象**，并设置它们的选项，每个对象都可以是不同的设置。

> 比如说我们有一些作为3D模型数据（一栋房子或一个人物）的容器对象，在我们想绘制其中任何一个模型的时候，只需绑定一个包含对应模型数据的对象就可以了（当然，我们需要先创建并设置对象的选项）。

* 拥有数个这样的对象允许我们指定多个模型，在想画其中任何一个的时候,，直接将对应的对象绑定上去，便**不需要再重复设置**选项了。





# 你好窗口

## 实例化窗口

```glsl
int main()
{
    glfwInit();
    glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
    glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
    glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
    //glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GL_TRUE);

    return 0;
}
```

> 首先，我们在main函数中调用glfwInit函数来初始化GLFW，然后我们可以使用glfwWindowHint函数来配置GLFW。glfwWindowHint函数的第一个参数代表选项的名称，我们可以从很多以`GLFW_`开头的枚举值中选择；第二个参数接受一个整型，用来设置这个选项的值。该函数的所有的选项以及对应的值都可以在 [GLFW’s window handling](http://www.glfw.org/docs/latest/window.html#window_hints) 这篇文档中找到。

## 创建窗口对象

* 这个窗口对象存放了所有和窗口相关的数据
* 会被GLFW的其他函数频繁地用到。

```glsl
GLFWwindow* window = glfwCreateWindow(800, 600, "LearnOpenGL", NULL, NULL);
if (window == NULL)
{
    std::cout << "Failed to create GLFW window" << std::endl;
    glfwTerminate();//终止
    return -1;
}
glfwMakeContextCurrent(window);
```

## 初始化GLAD

* GLAD是用来管理OpenGL的**函数指针**
* 我们给GLAD传入了用来加载系统相关的OpenGL函数**指针地址的函数。**

``` GLSL
if (!gladLoadGLLoader((GLADloadproc)glfwGetProcAddress))
{
    std::cout << "Failed to initialize GLAD" << std::endl;
    return -1;
}
```

### 视口 

* 告诉OpenGL渲染窗口的尺寸大小, 从而设置窗口的**维度**(Dimension)：

* 前两个参数控制窗口**左下角的位置**。第三个和第四个参数控制渲染窗口的**宽度和高度**（像素）

  ```glsl
  glViewport(0, 0, 800, 600);
  ```

> 可以将视口的维度设置为比GLFW的维度小，这样子之后所有的OpenGL渲染将会在一个**更小的窗口中显示**，这样子的话我们也可以将一些其它元素**显示在OpenGL视口之外**。

#### 回调函数(Callback Function)

* 用户改变窗口的大小的时候，视口也应该被调整。我们可以对窗口注册一个**回调函数(Callback Function)**,它会在每次窗口大小被调整的时候被调用。这个回调函数的原型如下：

  ```glsl
  void framebuffer_size_callback(GLFWwindow* window, int width, int height);
  ```

* 每当窗口改变大小，GLFW会调用这个函数并填充相应的参数供你处理。

```glsl 
void framebuffer_size_callback(GLFWwindow* window, int width, int height)
{
    glViewport(0, 0, width, height);
}
```

#### 注册

* 我们还需要**注册**这个函数，告诉GLFW我们希望每当窗口调整大小的时候调用这个函数：

  ```glsl
  glfwSetFramebufferSizeCallback(window, framebuffer_size_callback);
  ```

  > 当窗口被**第一次显示**的时候**framebuffer_size_callback**也会被调用

* 我们还可以将我们的函数注册到其它很多的回调函数中。比如说，我们可以创建一个回调函数来**处理手柄输入变化，处理错误消息**等。我们会在**创建窗口之后**，**渲染循环初始化之前**注册这些回调函数。

## 准备好你的引擎

### 渲染循环(Render Loop)

* 我们需要在程序中添加一个**while循环**,它能在我们让GLFW退出前一直保持运行

* 下面几行的代码就实现了一个简单的渲染循环：

  ``` GLSL
  while(!glfwWindowShouldClose(window))//检查是否被要求退出
  {
      glfwSwapBuffers(window);//交换颜色缓冲（它是一个储存着GLFW窗口每一个像素颜色值的大缓冲）
      glfwPollEvents();    //检查是否触发事件（比如键盘输入、鼠标移动等）、更新窗口状态，并调用对应的回调函数
  }
  ```

#### 双缓冲

* 使用单缓冲绘图时可能会存在图像闪烁的问题
* 生成的图像不是一下子被绘制出来的，而是按照从左到右，由上而下逐像素地绘制而成的。
* **前缓冲**保存着最终输出的图像，它会在屏幕上显示
* 而所有的的渲染指令都会在**后缓冲**上绘制

* 所有的渲染指令执行完毕后，我们**交换**(Swap)前缓冲和后缓冲，这样图像就立即呈显出来，之前提到的不真实感就消除了.

## 最后一件事

### 释放资源

* 当渲染循环结束后我们需要**正确释放/删除**之前的分配的所有资源。我们可以在main函数的最后调用glfwTerminate函数来完成。

  ```GLSL
  glfwTerminate();
  return 0;
  ```

* 如果没做错的话，你将会看到如下的输出：

![img](D:\Program\OpenGL\LearnOpenGL\OpenGL学习.assets\hellowindow.png)



## 输入

### `glfwGetKey`

* 这个函数将会返回这个按键是否正在被按下。

```glsl
void processInput(GLFWwindow *window)
{
    if(glfwGetKey(window, GLFW_KEY_ESCAPE) == GLFW_PRESS)
        glfwSetWindowShouldClose(window, true);
}
```

> 这里我们检查用户是否按下了**返回键(Esc)**

* 如果没有按下，glfwGetKey将会返回**GLFW_RELEASE**

* 我们将通过glfwSetwindowShouldClose使用`WindowShouldClose`属性设置为 `true`的方法关闭GLFW



### 接下来在渲染循环的每一个迭代中调用processInput

```glsl
while (!glfwWindowShouldClose(window))
{
    processInput(window);

    glfwSwapBuffers(window);
    glfwPollEvents();
}
```





## 渲染

#### 使用自定义的颜色清空屏幕`glClear`

* 在每个新的渲染迭代开始的时候我们总是希望清屏
* 否则我们仍能看见上一次迭代的渲染结果

* glClear 接受一个**缓冲位(Buffer Bit)**来指定要清空的缓冲,可能的缓冲位有GL_COLOR_BUFFER_BIT，GL_DEPTH_BUFFER_BIT和GL_STENCIL_BUFFER_BIT。
* 由于现在我们只关心颜色值，所以我们只清空颜色缓冲。

```glsl
glClearColor(0.2f, 0.3f, 0.3f, 1.0f);
glClear(GL_COLOR_BUFFER_BIT);
```

### 
