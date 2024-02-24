# 01 Depth Testing深度测试

* **深度缓冲**在片段着色器运行之后
* **gl_FragCoord**的x和y分量代表了片段的屏幕空间坐标（其中(0, 0)位于左下角），gl_FragCoord的x和y分量代表了片段的屏幕空间坐标（其中(0, 0)位于左下角）

* 提前深度测试-GPU

* **不**希望更新深度缓冲--使用一个只读的(Read-only)深度缓冲。OpenGL允许我们禁用深度缓冲的写入，需要设置它的深度掩码(Depth Mask)设置为`GL_FALSE`就可以了：

  ```c++
  glDepthMask(GL_FALSE);
  ```

  这只在深度测试被启用的时候才有效果。

## 深度测试函数

可以调用glDepthFunc函数来设置比较运算符（或者说深度函数(Depth Function)）：

```c++
glDepthFunc(GL_LESS);
```

| 函数        | 描述                                         |
| :---------- | :------------------------------------------- |
| GL_ALWAYS   | 永远通过深度测试                             |
| GL_NEVER    | 永远不通过深度测试                           |
| GL_LESS     | 在片段深度值小于缓冲的深度值时通过测试       |
| GL_EQUAL    | 在片段深度值等于缓冲区的深度值时通过测试     |
| GL_LEQUAL   | 在片段深度值小于等于缓冲区的深度值时通过测试 |
| GL_GREATER  | 在片段深度值大于缓冲区的深度值时通过测试     |
| GL_NOTEQUAL | 在片段深度值不等于缓冲区的深度值时通过测试   |
| GL_GEQUAL   | 在片段深度值大于等于缓冲区的深度值时通过测试 |

## 深度值精度

这个（线性）方程将z值变换到了0.0到1.0之间的深度值：

![image-20230817023751435](D:\Program\OpenGL\LearnOpenGL\高级openGL.assets\image-20230817023751435.png)

然而实践中是几乎永远不会使用这样的线性深度缓冲(Linear Depth Buffer)的，正确的投影性质，需要使用一个非线性的深度方程，它是与 1/z 成正比的。（**1000单位远的深度值和只有1单位远的充满细节的物体不需要使用相同的精度**）：

![image-20230817024200803](D:\Program\OpenGL\LearnOpenGL\高级openGL.assets\image-20230817024200803.png)

![img](D:\Program\OpenGL\LearnOpenGL\高级openGL.assets\depth_non_linear_graph.png)

## 深度缓冲的可视化

我们可以根据片段的深度值返回一个颜色向量来完成这一工作：

```c++
void main()
{
    FragColor = vec4(vec3(gl_FragCoord.z), 1.0);
}
```

---

我们也可以让片段非线性的深度值变换为线性的。要实现这个，我们需要仅仅反转深度值的投影变换。

* 首先将深度值从[0, 1]范围重新变换到[-1, 1]范围的标准化设备坐标（裁剪空间）：

  ```c++
  float z = depth * 2.0 - 1.0;
  ```

* 像投影矩阵那样反转这个非线性方程（方程2），并将这个反转的方程应用到最终的深度值上：

  ```c++
  float linearDepth = (2.0 * near * far) / (far + near - z * (far - near));
  ```

## 深度冲突（Z-fighting）

两个平面或者三角形非常紧密地平行排列在一起时会发生

# 02 模板测试 Stencil testing

* 模板测试是根据又一个缓冲来进行的，它叫做**模板缓冲(Stencil Buffer)**，我们可以在渲染的时候更新它来获得一些很有意思的效果。
* （通常）每个**模板值(Stencil Value)是**8位的。所以每个像素/片段一共能有256种不同的模板值

* 模板缓冲首先会被清除为0，之后在模板缓冲中使用1填充了一个空心矩形。场景中的片段将会只在片段的模板值为1的时候会被渲染（其它的都被丢弃了）

启用GL_STENCIL_TEST来启用模板测试。

```c++
glEnable(GL_STENCIL_TEST);
```

和颜色和深度缓冲一样，你也需要在每次迭代之前清除模板缓冲。

```c++
glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT | GL_STENCIL_BUFFER_BIT);
```

lStencilMask允许我们设置一个位掩码(Bitmask)，它会与将要写入缓冲的模板值进行与(AND)运算。默认情况下设置的位掩码所有位都为1，不影响输出，但如果我们将它设置为`0x00`，写入缓冲的所有模板值最后都会变成0.这与深度测试中的glDepthMask(GL_FALSE)是等价的。

```c++
glStencilMask(0xFF); // 每一位写入模板缓冲时都保持原样
glStencilMask(0x00); // 每一位在写入模板缓冲时都会变成0（禁用写入）
```

## 模板函数

我们对模板缓冲应该通过还是失败，以及它应该如何影响模板缓冲，也是有一定控制的。

glStencilFunc(GLenum func, GLint ref, GLuint mask)一共包含三个参数，在一开始的那个简单的模板例子中，函数被设置为：

```c++
glStencilFunc(GL_EQUAL, 1, 0xFF)
```

这会告诉OpenGL，只要一个片段的模板值等于(`GL_EQUAL`)参考值1，片段将会通过测试并被绘制，否则会被丢弃

---

如何更新缓冲？这就需要glStencilOp这个函数了。

默认情况下glStencilOp是设置为`(GL_KEEP, GL_KEEP, GL_KEEP)`的，所以不论任何测试的结果是如何，模板缓冲都会保留它的值。默认的行为不会更新模板缓冲，所以如果你想写入模板缓冲的话，你需要至少对其中一个选项设置不同的值。

# 物体轮廓

我们将会展示一个使用模板测试就可以完成的有用特性，它叫做**物体轮廓(Object Outlining)。**

为物体创建轮廓的步骤如下：

1. 在绘制（需要添加轮廓的）物体之前，将模板函数设置为GL_ALWAYS，每当物体的片段被渲染时，将模板缓冲更新为1。
2. 渲染物体。
3. 禁用模板写入以及深度测试。
4. 将每个物体缩放一点点。
5. 使用一个不同的片段着色器，输出一个单独的（边框）颜色。
6. 再次绘制物体，但只在它们片段的模板值不等于1时才绘制。
7. 再次启用模板写入和深度测试。

## 边框

设置颜色

```c++
void main()
{
    FragColor = vec4(0.04, 0.28, 0.26, 1.0);
}
```

首先启用模板测试，并设置测试通过或失败时的行为：

```c++
glEnable(GL_STENCIL_TEST);
glStencilOp(GL_KEEP, GL_KEEP, GL_REPLACE);
```

画地板

```
glStencilMask(0x00); // 记得保证我们在绘制地板的时候不会更新模板缓冲
normalShader.use();
DrawFloor()  
```

我们将模板缓冲清除为0，对箱子中所有绘制的片段，将模板值更新为1：

```c++
glStencilFunc(GL_ALWAYS, 1, 0xFF); // 所有的片段都应该更新模板缓冲
glStencilMask(0xFF); // 启用模板缓冲写入
normalShader.use();
DrawTwoContainers();
```

现在模板缓冲在箱子被绘制的地方都更新为1了，我们将要绘制放大的箱子，但这次要禁用模板缓冲的写入：

```c++
glStencilFunc(GL_NOTEQUAL, 1, 0xFF);
glStencilMask(0x00); // 禁止模板缓冲的写入
glDisable(GL_DEPTH_TEST);
shaderSingleColor.use(); 
DrawTwoScaledUpContainers();
```

最后重新启用深度缓冲

```
glStencilMask(0xFF);
glEnable(GL_DEPTH_TEST);  
```

最后得出这样的效果

![image-20230824221645529](D:\Program\OpenGL\LearnOpenGL\高级openGL.assets\image-20230824221645529.png)

## 总结

在研究过程中发现了一些很有意思的东西，可以看出自己做出的结果和learnOpenGl的效果有点不一样，这是因为：

* 我做的不是地板，而是灯，灯可以覆盖在方块前，这样就会导致深度测试时，蓝色方块没通过。
* 此时GL_NOTEQUAL就会没有此前的模板缓冲而写入，因此灯覆盖在蓝色方块上就会显示为绿色
* 解决方法：
  * 换成地面，永远会被方块覆盖
  * 提前glDisable(GL_DEPTH_TEST)，使得方块永远画在灯上面![image-20230824222208519](D:\Program\OpenGL\LearnOpenGL\高级openGL.assets\image-20230824222208519.png)

# 03 混合

## 丢弃片段

`stb_image`在纹理有alpha通道的时候会自动加载，但我们仍要在纹理生成过程中告诉OpenGL，我们的纹理现在使用alpha通道了：

```c++
glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, data);
```

同样，保证你在片段着色器中获取了纹理的全部4个颜色分量，而不仅仅是RGB分量：

```c++
void main()
{
    // FragColor = vec4(vec3(texture(texture1, TexCoords)), 1.0);
    FragColor = texture(texture1, TexCoords);
}
```

创建一个vector，向里面添加几个`glm::vec3`变量来代表草的位置：

```c++
vector<glm::vec3> vegetation;
vegetation.push_back(glm::vec3(-1.5f,  0.0f, -0.48f));
vegetation.push_back(glm::vec3( 1.5f,  0.0f,  0.51f));
vegetation.push_back(glm::vec3( 0.0f,  0.0f,  0.7f));
vegetation.push_back(glm::vec3(-0.3f,  0.0f, -2.3f));
vegetation.push_back(glm::vec3( 0.5f,  0.0f, -0.6f));
```

因为草的纹理是添加到四边形对象上的，我们还需要创建另外一个VAO，填充VBO，设置正确的顶点属性指针。接下来，在绘制完地板和两个立方体后，我们将会绘制草：

```c++
glBindVertexArray(vegetationVAO);
glBindTexture(GL_TEXTURE_2D, grassTexture);  
for(unsigned int i = 0; i < vegetation.size(); i++) 
{
    model = glm::mat4(1.0f);
    model = glm::translate(model, vegetation[i]);               
    shader.setMat4("model", model);
    glDrawArrays(GL_TRIANGLES, 0, 6);
}
```

GLSL给了我们`discard`命令，一旦被调用，它就会保证片段不会被进一步处理，所以就不会进入颜色缓冲。有了这个指令，我们就能够在片段着色器中检测一个片段的alpha值是否低于某个阈值，如果是的话，则丢弃这个片段，就好像它不存在一样：

```c++
#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D texture1;

void main()
{             
    vec4 texColor = texture(texture1, TexCoords);
    if(texColor.a < 0.1)
        discard;
    FragColor = texColor;
}
```

这里，我们检测被采样的纹理颜色的alpha值是否低于0.1的阈值，如果是的话，则丢弃这个片段。片段着色器保证了它只会渲染不是（几乎）完全透明的片段。现在它看起来就正常了：

### 纹理边缘

OpenGL会对边缘的值和纹理下一个重复的值进行插值（因为我们将它的环绕方式设置为了GL_REPEAT。这通常是没问题的，但是由于我们使用了透明值，纹理图像的顶部将会与底部边缘的纯色值进行插值。这样的结果是一个半透明的有色边框。![image-20230825210945369](D:\Program\OpenGL\LearnOpenGL\高级openGL.assets\image-20230825210945369.png)

要想避免这个，每当你alpha纹理的时候，请将纹理的环绕方式设置为GL_CLAMP_TO_EDGE：

```
glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
```

## 混合

要想渲染有多个透明度级别的图像，我们需要启用混合(Blending)。和OpenGL大多数的功能一样，我们可以启用GL_BLEND来启用混合：

```c++
glEnable(GL_BLEND);
```

### 混合方程

OpenGL中的混合是通过下面这个方程来实现的：

![image-20230825080608274](D:\Program\OpenGL\LearnOpenGL\高级openGL.assets\image-20230825080608274.png)

为了获得之前两个方形的混合结果，我们需要使用源颜色向量的*a**l**p**h**a*作为源因子，使用1−*a**l**p**h**a*作为目标因子。这将会产生以下的glBlendFunc：

```c++
glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
```

也可以使用glBlendFuncSeparate为RGB和alpha通道分别设置不同的选项：

```c++
glBlendFuncSeparate(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA, GL_ONE, GL_ZERO);
```

OpenGL甚至给了我们更多的灵活性，允许我们改变方程中源和目标部分的运算符。glBlendEquation(GLenum mode)允许我们设置运算符，它提供了三个选项,默认是glBlendEquation。

我们可以省略调用glBlendEquation，因为GL_FUNC_ADD对大部分的操作来说都是我们希望的混合方程

## 渲染半透明纹理

首先，在初始化时我们启用混合，并设定相应的混合函数：

```c++
glEnable(GL_BLEND);
glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
```

由于启用了混合，我们就不需要丢弃片段了，所以我们把片段着色器还原：

```c++
#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D texture1;

void main()
{             
    FragColor = texture(texture1, TexCoords);
}
```

## 不要打乱顺序

当绘制一个有不透明和透明物体的场景的时候，大体的原则如下：

1. 先绘制所有不透明的物体。
2. 对所有透明的物体排序。
3. 按顺序绘制所有透明的物体。

map会自动根据键值(Key)对它的值排序，所以只要我们添加了所有的位置，并以它的距离作为键，它们就会自动根据距离值排序了。

```c++
std::map<float, glm::vec3> sorted;
for (unsigned int i = 0; i < windows.size(); i++)
{
    float distance = glm::length(camera.Position - windows[i]);
    sorted[distance] = windows[i];
}
```

结果就是一个排序后的容器对象，它根据distance键值从低到高储存了每个窗户的位置。

之后，这次在渲染的时候，我们将以逆序（从远到近）从map中获取值，之后以正确的顺序绘制对应的窗户：

```c++
for(std::map<float,glm::vec3>::reverse_iterator it = sorted.rbegin(); it != sorted.rend(); ++it) 
{
    model = glm::mat4();
    model = glm::translate(model, it->second);              
    shader.setMat4("model", model);
    glDrawArrays(GL_TRIANGLES, 0, 6);
}
```

我们使用了map的一个**反向迭代器(Reverse Iterator)**，反向遍历其中的条目，并将每个窗户四边形位移到对应的窗户位置上。这是排序透明物体的一个比较简单的实现，它能够修复之前的问题

# 04 面剔除 Face culling

## 环绕顺序

默认情况下，逆时针顶点所定义的三角形将会被处理为正向三角形。

![img](D:\Program\OpenGL\LearnOpenGL\高级openGL.assets\faceculling_windingorder.png)

在顶点数据中，我们定义的是两个逆时针顺序的三角形。虽然背面的三角形是以逆时针定义的，它现在是以顺时针顺序渲染的了。这正是我们想要剔除（Cull，丢弃）的不可见面了！

## 面剔除

> 在之前教程中使用的立方体顶点数据并不是按照逆时针环绕顺序定义的
>
> 要使用**面剔除**，需要确保逆时针定义三角形

要想启用面剔除，我们只需要启用OpenGL的GL_CULL_FACE选项：

```c++
glEnable(GL_CULL_FACE);
```

OpenGL允许我们改变需要剔除的面的类型。如果我们只想剔除正向面而不是背向面会怎么样？我们可以调用glCullFace来定义这一行为：

```c++
glCullFace(GL_FRONT);
```

glCullFace的初始值是GL_BACK。除了需要剔除的面之外，我们也可以通过调用glFrontFace，告诉OpenGL我们希望将顺时针的面（而不是逆时针的面）定义为正向面：

```c++
glFrontFace(GL_CCW);
```

# 05 帧缓冲 Framebuffers

用于写入颜色值的颜色缓冲、用于写入深度信息的深度缓冲和允许我们根据一些条件丢弃特定片段的模板缓冲。这些缓冲结合起来叫做帧缓冲(Framebuffer)

## 屏幕纹理

首先，需要在屏幕上画一个四边形

```
    float quadVertices[] = { // vertex attributes for a quad that fills the entire screen in Normalized Device Coordinates.
        // positions   // texCoords
        -1.0f,  1.0f,  0.0f, 1.0f,
        -1.0f, -1.0f,  0.0f, 0.0f,
         1.0f, -1.0f,  1.0f, 0.0f,

        -1.0f,  1.0f,  0.0f, 1.0f,
         1.0f, -1.0f,  1.0f, 0.0f,
         1.0f,  1.0f,  1.0f, 1.0f
    };
```

对这个四边形进行VAO和VBO的绑定



## 创建一个帧缓冲

和OpenGL中的其它对象一样，我们会使用一个叫做glGenFramebuffers的函数来创建一个帧缓冲对象(Framebuffer Object, FBO)：

```c++
unsigned int fbo;
glGenFramebuffers(1, &fbo);
```

检查帧缓冲是否完整

```c++
if(glCheckFramebufferStatus(GL_FRAMEBUFFER) == GL_FRAMEBUFFER_COMPLETE)
  // 执行胜利的舞蹈
```

之后所有的渲染操作将会渲染到当前绑定帧缓冲的附件中。由于我们的帧缓冲不是默认帧缓冲，渲染指令将不会对窗口的视觉输出有任何影响。

出于这个原因，渲染到一个不同的帧缓冲被叫做**离屏渲染(Off-screen Rendering)**。要保证所有的渲染操作在主窗口中有视觉效果，我们需要再次激活默认帧缓冲，将它绑定到`0`

在完成所有的帧缓冲操作之后，不要忘记删除这个帧缓冲对象：

```c++
glDeleteFramebuffers(1, &fbo);
```

在完整性检查执行之前，我们需要给帧缓冲附加一个附件。当创建一个附件的时候我们有两个选项：纹理或渲染缓冲对象(Renderbuffer Object)。

### 纹理附件

为帧缓冲创建一个纹理和创建一个普通的纹理差不多：

```c++
unsigned int texture;
glGenTextures(1, &texture);
glBindTexture(GL_TEXTURE_2D, texture);

glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, 800, 600, 0, GL_RGB, GL_UNSIGNED_BYTE, NULL);

glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
```

* 如果你想将你的屏幕渲染到一个更小或更大的纹理上，你需要（在渲染到你的帧缓冲之前）再次**调用glViewport**，使用纹理的新维度作为参数，否则只有一小部分的纹理或屏幕会被渲染到这个纹理上。

我们已经创建好一个纹理了，要做的最后一件事就是将它附加到帧缓冲上了：

```c++
glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, texture, 0);
```

将一个深度和模板缓冲附加为一个纹理到帧缓冲的例子可以在下面找到：

```c++
glTexImage2D(
  GL_TEXTURE_2D, 0, GL_DEPTH24_STENCIL8, 800, 600, 0, 
  GL_DEPTH_STENCIL, GL_UNSIGNED_INT_24_8, NULL
);

glFramebufferTexture2D(GL_FRAMEBUFFER, GL_DEPTH_STENCIL_ATTACHMENT, GL_TEXTURE_2D, texture, 0);
```

### 渲染缓冲对象附件	

创建一个渲染缓冲对象的代码和帧缓冲的代码很类似：

```c++
unsigned int rbo;
glGenRenderbuffers(1, &rbo);
```

类似，我们需要绑定这个渲染缓冲对象，让之后所有的渲染缓冲操作影响当前的rbo：

```c++
glBindRenderbuffer(GL_RENDERBUFFER, rbo);
```

最后一件事就是附加这个渲染缓冲对象：

```c++
glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_STENCIL_ATTACHMENT, GL_RENDERBUFFER, rbo);
```

## 渲染到纹理

首先要创建一个帧缓冲对象，并绑定它，这些都很直观：

```c++
unsigned int framebuffer;
glGenFramebuffers(1, &framebuffer);
glBindFramebuffer(GL_FRAMEBUFFER, framebuffer);
```

接下来我们需要创建一个纹理图像，我们将它作为一个颜色附件附加到帧缓冲上。我们将纹理的维度设置为窗口的宽度和高度，并且不初始化它的数据：

```c++
// 生成纹理
unsigned int texColorBuffer;
glGenTextures(1, &texColorBuffer);
glBindTexture(GL_TEXTURE_2D, texColorBuffer);
glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, 800, 600, 0, GL_RGB, GL_UNSIGNED_BYTE, NULL);
glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR );
glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
glBindTexture(GL_TEXTURE_2D, 0);
	g
// 将它附加到当前绑定的帧缓冲对象
glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, texColorBuffer, 0);  
```

创建一个渲染缓冲对象不是非常复杂。我们需要记住的唯一事情是，我们将它创建为一个深度**和**模板附件渲染缓冲对象。我们将它的**内部**格式设置为GL_DEPTH24_STENCIL8，对我们来说这个精度已经足够了。

```c++
unsigned int rbo;
glGenRenderbuffers(1, &rbo);
glBindRenderbuffer(GL_RENDERBUFFER, rbo); 
glRenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH24_STENCIL8, 800, 600);  
glBindRenderbuffer(GL_RENDERBUFFER, 0);
```

作为完成帧缓冲之前的最后一步，我们将渲染缓冲对象附加到帧缓冲的深度**和**模板附件上：

```c++
glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_STENCIL_ATTACHMENT, GL_RENDERBUFFER, rbo);
```

最后，我们希望检查帧缓冲是否是完整的，如果不是，我们将打印错误信息。

```c++
if(glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
    std::cout << "ERROR::FRAMEBUFFER:: Framebuffer is not complete!" << std::endl;
glBindFramebuffer(GL_FRAMEBUFFER, 0);
```

记得要解绑帧缓冲，保证我们不会不小心渲染到错误的帧缓冲上。

### 着色器

顶点着色器是这样的：

```c++
#version 330 core
layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aTexCoords;

out vec2 TexCoords;

void main()
{
    gl_Position = vec4(aPos.x, aPos.y, 0.0, 1.0); 
    TexCoords = aTexCoords;
}
```

并没有太复杂的东西。片段着色器会更加基础，我们做的唯一一件事就是从纹理中采样：

```c++
#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D screenTexture;

void main()
{ 
    FragColor = texture(screenTexture, TexCoords);
}
```

记得先载入纹理uniform

```
screenShader.use();
screenShader.setInt("screenTexture",0);
```

接着就靠你来为屏幕四边形创建并配置一个VAO了。帧缓冲的一个渲染迭代将会有以下的结构：

```c++
// 第一处理阶段(Pass)
glBindFramebuffer(GL_FRAMEBUFFER, framebuffer);
glClearColor(0.1f, 0.1f, 0.1f, 1.0f);
glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT); // 我们现在不使用模板缓冲
glEnable(GL_DEPTH_TEST);
DrawScene();    

// 第二处理阶段
glBindFramebuffer(GL_FRAMEBUFFER, 0); // 返回默认
glClearColor(1.0f, 1.0f, 1.0f, 1.0f); 
glClear(GL_COLOR_BUFFER_BIT);

glActiveTexture(GL_TEXTURE2);//注意sampler纹理绑定的序号，不要冲突了

screenShader.use();  
glBindVertexArray(quadVAO);
glDisable(GL_DEPTH_TEST);
glBindTexture(GL_TEXTURE_2D, textureColorbuffer);
glDrawArrays(GL_TRIANGLES, 0, 6);  
```

## 后期处理Post-Processing

### 反相

```c++
void main()
{
    FragColor = vec4(vec3(1.0 - texture(screenTexture, TexCoords)), 1.0);
}
```

![image-20230901104438126](D:\Program\OpenGL\LearnOpenGL\高级openGL.assets\image-20230901104438126.png)

### 灰度

另外一个很有趣的效果是，移除场景中除了黑白灰以外所有的颜色，让整个图像灰度化(Grayscale)。很简单的实现方式是，取所有的颜色分量，将它们平均化：

```c++
void main()
{
    FragColor = texture(screenTexture, TexCoords);
    float average = (FragColor.r + FragColor.g + FragColor.b) / 3.0;
    FragColor = vec4(average, average, average, 1.0);
}
```

这已经能创造很好的结果了，但人眼会对绿色更加敏感一些，而对蓝色不那么敏感，所以为了获取物理上更精确的效果，我们需要使用加权的(Weighted)通道：

```c++
void main()
{
    FragColor = texture(screenTexture, TexCoords);
    float average = 0.2126 * FragColor.r + 0.7152 * FragColor.g + 0.0722 * FragColor.b;
    FragColor = vec4(average, average, average, 1.0);
}
```

![image-20230901105027876](D:\Program\OpenGL\LearnOpenGL\高级openGL.assets\image-20230901105027876.png)

## 核效果

### 锐化Sharpen

我们假设使用的核都是3x3核（实际上大部分核都是）：

```c++
const float offset = 1.0 / 300.0;  

void main()
{
    vec2 offsets[9] = vec2[](
        vec2(-offset,  offset), // 左上
        vec2( 0.0f,    offset), // 正上
        vec2( offset,  offset), // 右上
        vec2(-offset,  0.0f),   // 左
        vec2( 0.0f,    0.0f),   // 中
        vec2( offset,  0.0f),   // 右
        vec2(-offset, -offset), // 左下
        vec2( 0.0f,   -offset), // 正下
        vec2( offset, -offset)  // 右下
    );

    float kernel[9] = float[](
        -1, -1, -1,
        -1,  9, -1,
        -1, -1, -1
    );

    vec3 sampleTex[9];
    for(int i = 0; i < 9; i++)
    {
        sampleTex[i] = vec3(texture(screenTexture, TexCoords.st + offsets[i]));
    }
    vec3 col = vec3(0.0);
    for(int i = 0; i < 9; i++)
        col += sampleTex[i] * kernel[i];

    FragColor = vec4(col, 1.0);
}
```

![image-20230901113405184](D:\Program\OpenGL\LearnOpenGL\高级openGL.assets\image-20230901113405184.png)

### 模糊 Blur

![image-20230901114327907](D:\Program\OpenGL\LearnOpenGL\高级openGL.assets\image-20230901114327907.png)

由于所有值的和是16，所以直接返回合并的采样颜色将产生非常亮的颜色，所以我们需要将核的每个值都除以16。最终的核数组将会是：

```c++
float kernel[9] = float[](
    1.0 / 16, 2.0 / 16, 1.0 / 16,
    2.0 / 16, 4.0 / 16, 2.0 / 16,
    1.0 / 16, 2.0 / 16, 1.0 / 16  
);
```

![image-20230901114725656](D:\Program\OpenGL\LearnOpenGL\高级openGL.assets\image-20230901114725656.png)

### 边缘检测 Edge-Detection

![image-20230901115149395](D:\Program\OpenGL\LearnOpenGL\高级openGL.assets\image-20230901115149395.png)

![image-20230901115004587](D:\Program\OpenGL\LearnOpenGL\高级openGL.assets\image-20230901115004587.png)

# 06 立方体贴图 Cubemaps

## 创建立方体贴图

立方体贴图是和其它纹理一样的，所以如果想创建一个立方体贴图的话，我们需要生成一个纹理，并将其绑定到纹理目标上，之后再做其它的纹理操作。这次要绑定到GL_TEXTURE_CUBE_MAP：

```c++
unsigned int textureID;
glGenTextures(1, &textureID);
glBindTexture(GL_TEXTURE_CUBE_MAP, textureID);
```

遍历整个纹理目标：

```c++
int width, height, nrChannels;
unsigned char *data;  
for(unsigned int i = 0; i < textures_faces.size(); i++)
{
    data = stbi_load(textures_faces[i].c_str(), &width, &height, &nrChannels, 0);
    glTexImage2D(
        GL_TEXTURE_CUBE_MAP_POSITIVE_X + i, 
        0, GL_RGB, width, height, 0, GL_RGB, GL_UNSIGNED_BYTE, data
    );
}
```

## 加载天空盒

因为天空盒本身就是一个立方体贴图，加载天空盒和之前加载立方体贴图时并没有什么不同。为了加载天空盒，我们将使用下面的函数，它接受一个包含6个纹理路径的vector：

```c++
unsigned int loadCubemap(vector<std::string> faces)
{
    unsigned int textureID;
    glGenTextures(1, &textureID);
    glBindTexture(GL_TEXTURE_CUBE_MAP, textureID);

    int width, height, nrChannels;
    for (unsigned int i = 0; i < faces.size(); i++)
    {
        unsigned char *data = stbi_load(faces[i].c_str(), &width, &height, &nrChannels, 0);
        if (data)
        {
            glTexImage2D(GL_TEXTURE_CUBE_MAP_POSITIVE_X + i, 
                         0, GL_RGB, width, height, 0, GL_RGB, GL_UNSIGNED_BYTE, data
            );
            stbi_image_free(data);
        }
        else
        {
            std::cout << "Cubemap texture failed to load at path: " << faces[i] << std::endl;
            stbi_image_free(data);
        }
    }
    glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
    glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
    glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_WRAP_R, GL_CLAMP_TO_EDGE);

    return textureID;
}
```

之后，在调用这个函数之前，我们需要将合适的纹理路径按照立方体贴图枚举指定的顺序加载到一个vector中。

```c++
vector<std::string> faces
{
    "right.jpg",
    "left.jpg",
    "top.jpg",
    "bottom.jpg",
    "front.jpg",
    "back.jpg"
};
unsigned int cubemapTexture = loadCubemap(faces);
```

## 显示天空盒

由于天空盒是绘制在一个立方体上的，和其它物体一样，我们需要另一个VAO、VBO以及新的一组顶点。

```
float skyboxVertices[] = {
    // positions          
    -1.0f,  1.0f, -1.0f,
    -1.0f, -1.0f, -1.0f,
     1.0f, -1.0f, -1.0f,
     1.0f, -1.0f, -1.0f,
     1.0f,  1.0f, -1.0f,
    -1.0f,  1.0f, -1.0f,

    -1.0f, -1.0f,  1.0f,
    -1.0f, -1.0f, -1.0f,
    -1.0f,  1.0f, -1.0f,
    -1.0f,  1.0f, -1.0f,
    -1.0f,  1.0f,  1.0f,
    -1.0f, -1.0f,  1.0f,

     1.0f, -1.0f, -1.0f,
     1.0f, -1.0f,  1.0f,
     1.0f,  1.0f,  1.0f,
     1.0f,  1.0f,  1.0f,
     1.0f,  1.0f, -1.0f,
     1.0f, -1.0f, -1.0f,

    -1.0f, -1.0f,  1.0f,
    -1.0f,  1.0f,  1.0f,
     1.0f,  1.0f,  1.0f,
     1.0f,  1.0f,  1.0f,
     1.0f, -1.0f,  1.0f,
    -1.0f, -1.0f,  1.0f,

    -1.0f,  1.0f, -1.0f,
     1.0f,  1.0f, -1.0f,
     1.0f,  1.0f,  1.0f,
     1.0f,  1.0f,  1.0f,
    -1.0f,  1.0f,  1.0f,
    -1.0f,  1.0f, -1.0f,

    -1.0f, -1.0f, -1.0f,
    -1.0f, -1.0f,  1.0f,
     1.0f, -1.0f, -1.0f,
     1.0f, -1.0f, -1.0f,
    -1.0f, -1.0f,  1.0f,
     1.0f, -1.0f,  1.0f
};
```



要渲染天空盒的话，我们需要一组新的着色器，它们都不是很复杂。因为我们只有一个顶点属性，顶点着色器非常简单：

```c++
#version 330 core
layout (location = 0) in vec3 aPos;

out vec3 TexCoords;

uniform mat4 projection;
uniform mat4 view;

void main()
{
    TexCoords = aPos;
    gl_Position = projection * view * vec4(aPos, 1.0);
}
```

注意，顶点着色器中很有意思的部分是，我们将输入的位置向量作为输出给片段着色器的纹理坐标。片段着色器会将它作为输入来采样`samplerCube`：

```c++
#version 330 core
out vec4 FragColor;

in vec3 TexCoords;

uniform samplerCube skybox;

void main()
{    
    FragColor = texture(skybox, TexCoords);
}
```

绘制天空盒时，我们需要将它变为场景中的第一个渲染的物体，并且禁用深度写入。这样子天空盒就会永远被绘制在其它物体的背后了。

```c++
glDepthMask(GL_FALSE);
skyboxShader.use();
// ... 设置观察和投影矩阵
glBindVertexArray(skyboxVAO);
glBindTexture(GL_TEXTURE_CUBE_MAP, cubemapTexture);
glDrawArrays(GL_TRIANGLES, 0, 36);
glDepthMask(GL_TRUE);
// ... 绘制剩下的场景
```

我们希望天空盒是以玩家为中心的，这样不论玩家移动了多远，天空盒都不会变近:

```c++
glm::mat4 view = glm::mat4(glm::mat3(camera.GetViewMatrix()));
```

## 优化-提前深度测试

* 我们需要欺骗深度缓冲，让它认为天空盒有着最大的深度值1.0，只要它前面有一个物体，深度测试就会失败。

* **透视除法**是在顶点着色器运行之后执行的，将gl_Position的`xyz`坐标除以w分量。我们又从[深度测试](https://learnopengl-cn.github.io/04 Advanced OpenGL/01 Depth testing/)小节中知道，相除结果的z分量等于顶点的深度值。我们可以将输出位置的z分量等于它的w分量，让z分量永远等于1.0，这样子的话，当透视除法执行之后，z分量会变为`w / w = 1.0`

  ```c++
  void main()
  {
      TexCoords = aPos;
      vec4 pos = projection * view * vec4(aPos, 1.0);
      gl_Position = pos.xyww;
  }
  ```

* 还要改变一下深度函数，将它从默认的GL_LESS改为GL_LEQUAL。深度缓冲将会填充上天空盒的1.0值，所以我们需要保证天空盒在值小于或等于深度缓冲而不是小于时通过深度测试：

* ```
    glDepthFunc(GL_LEQUAL);
          cubeMapsShader.use();
          // ... set view and projection
          view = glm::mat4(glm::mat3(camera.GetViewMatrix()));
          cubeMapsShader.setMat4("view", view);
          cubeMapsShader.setMat4("projection", projection);
    
          glBindVertexArray(skyBoxVAO);
          glBindTexture(GL_TEXTURE_CUBE_MAP, cubemapTexture);
          glDrawArrays(GL_TRIANGLES, 0, 36);
          glDepthFunc(GL_LESS);
  ```

### 挡住模板缓冲

如果想要正确渲染模板缓冲，还是得再**模板缓冲前面**进行渲染，因为模板缓冲是没有经过深度测试的；

### 混合

玻璃是可以看到天空盒后面的，所以还得在混合前绘制天空盒



# 环境映射

## 反射

![img](D:\Program\OpenGL\LearnOpenGL\高级openGL.assets\cubemaps_reflection_theory.png)

改变箱子的片段着色器，让箱子有反射性：

```c++
#version 330 core
out vec4 FragColor;

in vec3 Normal;
in vec3 Position;

uniform vec3 cameraPos;
uniform samplerCube skybox;

void main()
{             
    vec3 I = normalize(Position - cameraPos);
    vec3 R = reflect(I, normalize(Normal));
    FragColor = vec4(texture(skybox, R).rgb, 1.0);
}
```

更新一下顶点着色器。

```c++
#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;

out vec3 Normal;
out vec3 Position;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    Normal = mat3(transpose(inverse(model))) * aNormal;
    Position = vec3(model * vec4(aPos, 1.0));
    gl_Position = projection * view * model * vec4(aPos, 1.0);
}
```

绑一下带向量新顶点和VAO、VBO

我们在渲染箱子之前先绑定立方体贴图纹理：

```c++
glBindVertexArray(cubeVAO);
glBindTexture(GL_TEXTURE_CUBE_MAP, skyboxTexture);          
glDrawArrays(GL_TRIANGLES, 0, 36);
```

![image-20230905202907613](D:\Program\OpenGL\LearnOpenGL\高级openGL.assets\image-20230905202907613.png)

人类：

![image-20230905213316284](D:\Program\OpenGL\LearnOpenGL\高级openGL.assets\image-20230905213316284.png)

### 折射

![img](D:\Program\OpenGL\LearnOpenGL\高级openGL.assets\cubemaps_refraction_theory.png)

* 尽量不要复用着色器![image-20230906090454843](D:\Program\OpenGL\LearnOpenGL\高级openGL.assets\image-20230906090454843.png)

### 动态环境贴图

* 帧缓冲
* 渲染六个面
* 开销大

### 反射贴图

# 07 高级数据

我们也可以使用glBufferSubData，填充缓冲的特定区域。这能够让我们插入或者更新缓冲内存的某一部分。

* 要注意的是，缓冲需要有足够的已分配内存，所以对一个缓冲调用glBufferSubData之前**必须要先调用glBufferData**。

```c++
glBufferSubData(GL_ARRAY_BUFFER, 24, sizeof(data), &data); // 范围： [24, 24 + sizeof(data)]
```

将数据导入缓冲的另外一种方法是，请求缓冲内存的指针，直接将数据复制到缓冲当中。通过调用**glMapBuffer函数**，OpenGL会返回当前绑定缓冲的内存指针，供我们操作：

```c++
float data[] = {
  0.5f, 1.0f, -0.35f
  ...
};
glBindBuffer(GL_ARRAY_BUFFER, buffer);
// 获取指针
void *ptr = glMapBuffer(GL_ARRAY_BUFFER, GL_WRITE_ONLY);
// 复制数据到内存
memcpy(ptr, data, sizeof(data));
// 记得告诉OpenGL我们不再需要这个指针了
glUnmapBuffer(GL_ARRAY_BUFFER);
```

## 分批顶点属性

当从文件中加载顶点数据的时候，你通常获取到的是一个位置数组、一个法线数组和/或一个纹理坐标数组。我们需要花点力气才能将这些数组转化为一个大的交错数据数组。

使用分批的方式会是更简单的解决方案，我们可以很容易使用glBufferSubData函数实现：

```c++
float positions[] = { ... };
float normals[] = { ... };
float tex[] = { ... };
// 填充缓冲
glBufferSubData(GL_ARRAY_BUFFER, 0, sizeof(positions), &positions);
glBufferSubData(GL_ARRAY_BUFFER, sizeof(positions), sizeof(normals), &normals);
glBufferSubData(GL_ARRAY_BUFFER, sizeof(positions) + sizeof(normals), sizeof(tex), &tex);
```

我们还需要更新顶点属性指针来反映这些改变：

```c++
glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 3 * sizeof(float), 0);  
glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, 3 * sizeof(float), (void*)(sizeof(positions)));  
glVertexAttribPointer(
  2, 2, GL_FLOAT, GL_FALSE, 2 * sizeof(float), (void*)(sizeof(positions) + sizeof(normals)));
```

## 复制缓冲

你可能会想与其它的缓冲共享其中的数据，或者想要将缓冲的内容复制到另一个缓冲当中。glCopyBufferSubData能够让我们相对容易地从一个缓冲中复制数据到另一个缓冲中。这个函数的原型如下：

```c++
void glCopyBufferSubData(GLenum readtarget, GLenum writetarget, GLintptr readoffset,
                         GLintptr writeoffset, GLsizeiptr size);
```

我们接下来就可以将需要的缓冲绑定到这两个缓冲目标上，并将这两个目标作为`readtarget`和`writetarget`参数。

接下来glCopyBufferSubData会从`readtarget`中读取`size`大小的数据，并将其写入`writetarget`缓冲的`writeoffset`偏移量处:

```c++
float vertexData[] = { ... };
glBindBuffer(GL_COPY_READ_BUFFER, vbo1);
glBindBuffer(GL_COPY_WRITE_BUFFER, vbo2);
glCopyBufferSubData(GL_COPY_READ_BUFFER, GL_COPY_WRITE_BUFFER, 0, 0, sizeof(vertexData))
```

我们也可以只将`writetarget`缓冲绑定为新的缓冲目标类型之一：

```c++
float vertexData[] = { ... };
glBindBuffer(GL_ARRAY_BUFFER, vbo1);
glBindBuffer(GL_COPY_WRITE_BUFFER, vbo2);
glCopyBufferSubData(GL_ARRAY_BUFFER, GL_COPY_WRITE_BUFFER, 0, 0, sizeof(vertexData));
```

# 08 高级GLSL

## 顶点着色器变量

## gl_Position

如知

### gl_PointSize

我们可以通过OpenGL的glPointSize函数来设置渲染出来的点的大小，但我们也可以在顶点着色器中修改这个值。

在顶点着色器中修改点大小的功能默认是禁用的，如果你需要启用它的话，你需要启用OpenGL的GL_PROGRAM_POINT_SIZE：

```c++
glEnable(GL_PROGRAM_POINT_SIZE);
```

一个简单的例子就是将点的大小设置为裁剪空间位置的z值，也就是顶点距观察者的距离。点的大小会随着观察者距顶点距离变远而增大。

```c++
void main()
{
    gl_Position = projection * view * model * vec4(aPos, 1.0);    
    gl_PointSize = gl_Position.z;    
}
```

### gl_VertexID 好像没用

顶点着色器还为我们提供了一个有趣的**输入变量**，我们只能对它进行读取，它叫做gl_VertexID。

整型变量gl_VertexID储存了正在绘制顶点的当前ID。当（使用glDrawElements）进行索引渲染的时候，这个变量会存储正在绘制顶点的当前索引。

当（使用glDrawArrays）不使用索引进行绘制的时候，这个变量会储存从渲染调用开始的已处理顶点数量。

## 片段着色器变量

### gl_FragCoord

gl_FragCoord的z分量等于对应片段的深度值

gl_FragCoord的x和y分量是片段的窗口空间(Window-space)坐标，其原点为窗口的左下角。我们已经使用glViewport设定了一个800x600的窗口了，所以片段窗口空间坐标的x分量将在0到800之间，y分量在0到600之间。

比如说，我们能够将屏幕分成两部分，在窗口的左侧渲染一种输出，在窗口的右侧渲染另一种输出。下面这个例子片段着色器会根据窗口坐标输出不同的颜色：

```c++
void main()
{             
    if(gl_FragCoord.x < 400)
        FragColor = vec4(1.0, 0.0, 0.0, 1.0);
    else
        FragColor = vec4(0.0, 1.0, 0.0, 1.0);        
}
```

### gl_FrontFacing

如果我们不（启用GL_FACE_CULL来）使用面剔除，那么gl_FrontFacing将会告诉我们当前片段是属于正向面的一部分还是背向面的一部分。举例来说，我们能够对正向面计算出不同的颜色。

gl_FrontFacing变量是一个bool，如果当前片段是正向面的一部分那么就是`true`，否则就是`false`。比如说，我们可以这样子创建一个立方体，在**内部和外部使用不同的纹理**：

```c++
#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D frontTexture;
uniform sampler2D backTexture;

void main()
{             
    if(gl_FrontFacing)
        FragColor = texture(frontTexture, TexCoords);
    else
        FragColor = texture(backTexture, TexCoords);
}
```

### gl_FragDepth

GLSL提供给我们一个叫做gl_FragDepth的输出变量，我们可以使用它来在着色器内设置片段的深度值。

要想设置深度值，我们直接写入一个0.0到1.0之间的float值到输出变量就可以了：

```c++
gl_FragDepth = 0.0; // 这个片段现在的深度值为 0.0
```

如果着色器没有写入值到gl_FragDepth，它会自动取用`gl_FragCoord.z`的值。

#### 缺陷

由我们自己设置深度值有一个很大的缺点，只要我们在片段着色器中对gl_FragDepth进行写入，OpenGL就会（像[深度测试](https://learnopengl-cn.github.io/04 Advanced OpenGL/01 Depth testing/)小节中讨论的那样）禁用所有的提前深度测试(Early Depth Testing)。它被禁用的原因是，OpenGL无法在片段着色器运行**之前**得知片段将拥有的深度值，因为片段着色器可能会完全修改这个深度值。

#### 补足

从OpenGL 4.2起，我们仍可以对两者进行一定的调和，在片段着色器的顶部使用深度条件(Depth Condition)重新声明gl_FragDepth变量：

```c++
layout (depth_<condition>) out float gl_FragDepth;
```

`condition`可以为下面的值：

| 条件        | 描述                                                         |
| :---------- | :----------------------------------------------------------- |
| `any`       | 默认值。提前深度测试是禁用的，你会损失很多性能               |
| `greater`   | 你只能让深度值比`gl_FragCoord.z`更大                         |
| `less`      | 你只能让深度值比`gl_FragCoord.z`更小                         |
| `unchanged` | 如果你要写入`gl_FragDepth`，你将只能写入`gl_FragCoord.z`的值 |

通过将深度条件设置为`greater`或者`less`，OpenGL就能假设你只会写入比当前片段深度值更大或者更小的值了。这样子的话，当深度值比片段的深度值要小的时候，OpenGL仍是能够进行提前深度测试的。

下面这个例子中，我们对片段的深度值进行了递增，但仍然也保留了一些提前深度测试：

```c++
#version 420 core // 注意GLSL的版本！
out vec4 FragColor;
layout (depth_greater) out float gl_FragDepth;

void main()
{             
    FragColor = vec4(1.0);
    gl_FragDepth = gl_FragCoord.z + 0.1;
}  
```

# 接口块

为了帮助我们管理这些变量，GLSL为我们提供了一个叫做接口块(Interface Block)的东西，来方便我们组合这些变量。接口块的声明和struct的声明有点相像，不同的是，现在根据它是一个输入还是输出块(Block)，使用in或out关键字来定义的。

```c++
#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoords;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out VS_OUT
{
    vec2 TexCoords;
} vs_out;

void main()
{
    gl_Position = projection * view * model * vec4(aPos, 1.0);    
    vs_out.TexCoords = aTexCoords;
}  
```



块名(Block Name)应该是和着色器中一样的（VS_OUT），但实例名(Instance Name)（顶点着色器中用的是vs_out）可以是随意的，但要避免使用误导性的名称，比如对实际上包含输入变量的接口块命名为vs_out。

```c++
#version 330 core
out vec4 FragColor;

in VS_OUT
{
    vec2 TexCoords;
} fs_in;

uniform sampler2D texture;

void main()
{             
    FragColor = texture(texture, fs_in.TexCoords);   
}
```

# Uniform缓冲对象

OpenGL为我们提供了一个叫做Uniform缓冲对象(Uniform Buffer Object)的工具，它允许我们定义一系列在多个着色器中相同的**全局**Uniform变量。当使用Uniform缓冲对象的时候，我们只需要设置相关的uniform**一次**。当然，我们仍需要手动设置每个着色器中不同的uniform。并且创建和配置Uniform缓冲对象会有一点繁琐。

首先，我们将使用一个简单的顶点着色器，将projection和view矩阵存储到所谓的Uniform块(Uniform Block)中：

```c++
#version 330 core
layout (location = 0) in vec3 aPos;

layout (std140) uniform Matrices
{
    mat4 projection;
    mat4 view;
};

uniform mat4 model;

void main()
{
    gl_Position = projection * view * model * vec4(aPos, 1.0);
}
```

在我们大多数的例子中，我们都会在每个渲染迭代中，对每个着色器设置**projection和view** Uniform矩阵。这是利用Uniform缓冲对象的一个非常完美的例子，因为现在我们只需要存储这些矩阵一次就可以了。

## Uniform块布局

假设着色器中有以下的这个Uniform块：

```c++
layout (std140) uniform ExampleBlock
{
    float value;
    vec3  vector;
    mat4  matrix;
    float values[3];
    bool  boolean;
    int   integer;
};
```

GLSL中的每个变量，比如说int、float和bool，都被定义为4字节量。每4个字节将会用一个`N`来表示。

| 类型                | 布局规则                                                     |
| :------------------ | :----------------------------------------------------------- |
| 标量，比如int和bool | 每个标量的基准对齐量为N。                                    |
| 向量                | 2N或者4N。这意味着vec3的基准对齐量为4N。                     |
| 标量或向量的数组    | 每个元素的基准对齐量与vec4的相同。                           |
| 矩阵                | 储存为列向量的数组，每个向量的基准对齐量与vec4的相同。       |
| 结构体              | 等于所有元素根据规则计算后的大小，但会填充到vec4大小的倍数。 |

使用std140布局计算出每个成员的对齐偏移量：

```c++
layout (std140) uniform ExampleBlock
{
                     // 基准对齐量       // 对齐偏移量
    float value;     // 4               // 0 
    vec3 vector;     // 16              // 16  (必须是16的倍数，所以 4->16)
    mat4 matrix;     // 16              // 32  (列 0)
                     // 16              // 48  (列 1)
                     // 16              // 64  (列 2)
                     // 16              // 80  (列 3)
    float values[3]; // 16              // 96  (values[0])
                     // 16              // 112 (values[1])
                     // 16              // 128 (values[2])
    bool boolean;    // 4               // 144
    int integer;     // 4               // 148
}; 										// 152
```

## 使用Uniform缓冲

```c++
unsigned int uboExampleBlock;
glGenBuffers(1, &uboExampleBlock);
glBindBuffer(GL_UNIFORM_BUFFER, uboExampleBlock);
glBufferData(GL_UNIFORM_BUFFER, 152, NULL, GL_STATIC_DRAW); // 分配152字节的内存
glBindBuffer(GL_UNIFORM_BUFFER, 0);
```

在OpenGL上下文中，定义了一些绑定点(Binding Point)，我们可以将一个Uniform缓冲链接至它。在创建Uniform缓冲之后，我们将它绑定到其中一个绑定点上，并将着色器中的Uniform块绑定到相同的绑定点，把它们连接到一起。下面的这个图示展示了这个：	

![img](D:\Program\OpenGL\LearnOpenGL\高级openGL.assets\advanced_glsl_binding_points.png)

为了将Uniform块绑定到一个特定的绑定点中，我们需要调用glUniformBlockBinding函数，它的第一个参数是一个程序对象，之后是一个Uniform块索引和链接到的绑定点。Uniform块索引(Uniform Block Index)是着色器中已定义Uniform块的位置值索引。这可以通过调用glGetUniformBlockIndex来获取，它接受一个程序对象和Uniform块的名称。我们可以用以下方式将图示中的Lights Uniform块链接到绑定点2：

```c++
unsigned int lights_index = glGetUniformBlockIndex(shaderA.ID, "Lights");   
glUniformBlockBinding(shaderA.ID, lights_index, 2);
```

我们需要对**每个**着色器重复这一步骤。

* 从OpenGL 4.2版本起，你也可以添加一个布局标识符，显式地将Uniform块的绑定点储存在着色器中，这样就不用再调用glGetUniformBlockIndex和glUniformBlockBinding了。下面的代码显式地设置了Lights Uniform块的绑定点。

```
layout(std140, binding = 2) uniform Lights { ... };
```

我们还需要绑定Uniform缓冲对象到相同的绑定点上，这可以使用glBindBufferBase或glBindBufferRange来完成。

```c++
glBindBufferBase(GL_UNIFORM_BUFFER, 2, uboExampleBlock); 
// 或
glBindBufferRange(GL_UNIFORM_BUFFER, 2, uboExampleBlock, 0, 152);
```

。只要我们需要，就可以使用glBufferSubData函数，用一个字节数组添加所有的数据，或者更新缓冲的一部分。要想更新uniform变量boolean，我们可以用以下方式更新Uniform缓冲对象：

```c++
glBindBuffer(GL_UNIFORM_BUFFER, uboExampleBlock);
int b = true; // GLSL中的bool是4字节的，所以我们将它存为一个integer
glBufferSubData(GL_UNIFORM_BUFFER, 144, 4, &b); 
glBindBuffer(GL_UNIFORM_BUFFER, 0);
```

## 一个简单的例子

我们会将投影和模型矩阵存储到一个叫做Matrices的Uniform块中。我们不会将模型矩阵存在这里，因为模型矩阵在不同的着色器中会不断改变，所以使用Uniform缓冲对象并不会带来什么好处。

```c++
#version 330 core
layout (location = 0) in vec3 aPos;

layout (std140) uniform Matrices
{
    mat4 projection;
    mat4 view;
};
uniform mat4 model;

void main()
{
    gl_Position = projection * view * model * vec4(aPos, 1.0);
}
```

首先，我们将顶点着色器的Uniform块设置为绑定点0。注意我们需要对每个着色器都设置一遍。

```c++
unsigned int uniformBlockIndexRed    = glGetUniformBlockIndex(shaderRed.ID, "Matrices");
unsigned int uniformBlockIndexGreen  = glGetUniformBlockIndex(shaderGreen.ID, "Matrices");
unsigned int uniformBlockIndexBlue   = glGetUniformBlockIndex(shaderBlue.ID, "Matrices");
unsigned int uniformBlockIndexYellow = glGetUniformBlockIndex(shaderYellow.ID, "Matrices");  

glUniformBlockBinding(shaderRed.ID,    uniformBlockIndexRed, 0);
glUniformBlockBinding(shaderGreen.ID,  uniformBlockIndexGreen, 0);
glUniformBlockBinding(shaderBlue.ID,   uniformBlockIndexBlue, 0);
glUniformBlockBinding(shaderYellow.ID, uniformBlockIndexYellow, 0);
```

接下来，我们创建Uniform缓冲对象本身，并将其绑定到绑定点0：

```c++
unsigned int uboMatrices
glGenBuffers(1, &uboMatrices);

glBindBuffer(GL_UNIFORM_BUFFER, uboMatrices);
glBufferData(GL_UNIFORM_BUFFER, 2 * sizeof(glm::mat4), NULL, GL_STATIC_DRAW);
glBindBuffer(GL_UNIFORM_BUFFER, 0);

glBindBufferRange(GL_UNIFORM_BUFFER, 0, uboMatrices, 0, 2 * sizeof(glm::mat4));
```

我们可以使用glBufferSubData在进入渲染循环之前存储投影矩阵：

```c++
glm::mat4 projection = glm::perspective(glm::radians(45.0f), (float)width/(float)height, 0.1f, 100.0f);
glBindBuffer(GL_UNIFORM_BUFFER, uboMatrices);
glBufferSubData(GL_UNIFORM_BUFFER, 0, sizeof(glm::mat4), glm::value_ptr(projection));
glBindBuffer(GL_UNIFORM_BUFFER, 0);
```

这里我们将投影矩阵储存在Uniform缓冲的前半部分。在每次渲染迭代中绘制物体之前，我们会将观察矩阵更新到缓冲的后半部分：

```c++
glm::mat4 view = camera.GetViewMatrix();           
glBindBuffer(GL_UNIFORM_BUFFER, uboMatrices);
glBufferSubData(GL_UNIFORM_BUFFER, sizeof(glm::mat4), sizeof(glm::mat4), glm::value_ptr(view));
glBindBuffer(GL_UNIFORM_BUFFER, 0);
```

Uniform缓冲对象的部分就结束了。每个包含了Matrices这个Uniform块的顶点着色器将会包含储存在uboMatrices中的数据。所以，如果我们现在要用4个不同的着色器绘制4个立方体，它们的投影和观察矩阵都会是一样的。

```c++
glBindVertexArray(cubeVAO);
shaderRed.use();
glm::mat4 model;
model = glm::translate(model, glm::vec3(-0.75f, 0.75f, 0.0f));  // 移动到左上角
shaderRed.setMat4("model", model);
glDrawArrays(GL_TRIANGLES, 0, 36);        
// ... 绘制绿色立方体
// ... 绘制蓝色立方体
// ... 绘制黄色立方体 
```

# 09 几何着色器

我们直接先看一个几何着色器的例子：

```c++
#version 330 core
layout (points) in;
layout (line_strip, max_vertices = 2) out;

void main() {    
    gl_Position = gl_in[0].gl_Position + vec4(-0.1, 0.0, 0.0, 0.0); 
    EmitVertex();

    gl_Position = gl_in[0].gl_Position + vec4( 0.1, 0.0, 0.0, 0.0);
    EmitVertex();

    EndPrimitive();
}
```

这个输入布局修饰符可以从顶点着色器接收下列任何一个图元值：

- `points`：绘制GL_POINTS图元时（1）。
- `lines`：绘制GL_LINES或GL_LINE_STRIP时（2）
- `lines_adjacency`：GL_LINES_ADJACENCY或GL_LINE_STRIP_ADJACENCY（4）
- `triangles`：GL_TRIANGLES、GL_TRIANGLE_STRIP或GL_TRIANGLE_FAN（3）
- `triangles_adjacency`：GL_TRIANGLES_ADJACENCY或GL_TRIANGLE_STRIP_ADJACENCY（6）

和输入布局修饰符一样，输出布局修饰符也可以接受几个图元值：

- `points`
- `line_strip`
- `triangle_strip`

为了生成更有意义的结果，我们需要某种方式来获取前一着色器阶段的输出。GLSL提供给我们一个内建(Built-in)变量，在内部看起来（可能）是这样的：

```c++
in gl_Vertex
{
    vec4  gl_Position;
    float gl_PointSize;
    float gl_ClipDistance[];
} gl_in[];
```

在我们的例子中，我们需要至少生成一个线条图元。

```c++
void main() {
    gl_Position = gl_in[0].gl_Position + vec4(-0.1, 0.0, 0.0, 0.0); 
    EmitVertex();

    gl_Position = gl_in[0].gl_Position + vec4( 0.1, 0.0, 0.0, 0.0);
    EmitVertex();

    EndPrimitive();
}
```

![img](D:\Program\OpenGL\LearnOpenGL\高级openGL.assets\geometry_shader_lines.png)

## 使用几何着色器

坐标是：

```c++
float points[] = {
    -0.5f,  0.5f, // 左上
     0.5f,  0.5f, // 右上
     0.5f, -0.5f, // 右下
    -0.5f, -0.5f  // 左下
};
```

顶点着色器只需要在z平面绘制点就可以了，所以我们将使用一个最基本顶点着色器：

```c++
#version 330 core
layout (location = 0) in vec2 aPos;

void main()
{
    gl_Position = vec4(aPos.x, aPos.y, 0.0, 1.0); 
}
```

直接在片段着色器中硬编码，将所有的点都输出为绿色：

```c++
#version 330 core
out vec4 FragColor;

void main()
{
    FragColor = vec4(0.0, 1.0, 0.0, 1.0);   
}
```

为点的顶点数据生成一个VAO和一个VBO，然后使用glDrawArrays进行绘制：

```c++
shader.use();
glBindVertexArray(VAO);
glDrawArrays(GL_POINTS, 0, 4);
```

结果是在黑暗的场景中有四个（很难看见的）绿点：

![img](D:\Program\OpenGL\LearnOpenGL\高级openGL.assets\geometry_shader_points.png)

出于学习目的，我们将会创建一个传递(Pass-through)几何着色器，它会接收一个点图元，并直接将它**传递**(Pass)到下一个着色器：

```c++
#version 330 core
layout (points) in;
layout (points, max_vertices = 1) out;

void main() {    
    gl_Position = gl_in[0].gl_Position; 
    EmitVertex();
    EndPrimitive();
}
```

和顶点与片段着色器一样，几何着色器也需要编译和链接，但这次在创建着色器时我们将会使用GL_GEOMETRY_SHADER作为着色器类型：

```c++
geometryShader = glCreateShader(GL_GEOMETRY_SHADER);
glShaderSource(geometryShader, 1, &gShaderCode, NULL);
glCompileShader(geometryShader);  
...
glAttachShader(program, geometryShader);
glLinkProgram(program);
```

## 造几个房子

![image-20230909134310916](D:\Program\OpenGL\LearnOpenGL\高级openGL.assets\image-20230909134310916.png)

# 爆破物体
