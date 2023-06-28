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
```

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

* 使用对象的一个好处是在程序中，我们不止可以定义一个对象，并设置它们的选项，每个对象都可以是不同的设置。

> 比如说我们有一些作为3D模型数据（一栋房子或一个人物）的容器对象，在我们想绘制其中任何一个模型的时候，只需绑定一个包含对应模型数据的对象就可以了（当然，我们需要先创建并设置对象的选项）。

* 拥有数个这样的对象允许我们指定多个模型，在想画其中任何一个的时候,，直接将对应的对象绑定上去，便不需要再重复设置选项了。