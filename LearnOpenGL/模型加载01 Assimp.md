# Assimp模型加载库

3D建模工具(3D Modeling Tool)可以让艺术家创建复杂的形状，并使用一种叫做UV映射(uv-mapping)的手段来应用贴图。这些工具将会在导出到模型文件的时候自动生成所有的顶点坐标、顶点法线以及纹理坐标。这样子艺术家们即使不了解图形技术细节的情况下，也能拥有一套强大的工具来构建高品质的模型了。所有的技术细节都隐藏在了导出的模型文件中。

* 但是，作为图形开发者，我们就**必须**要了解这些技术细节了。

我们的工作就是解析这些导出的模型文件以及提取所有有用的信息，将它们储存为OpenGL能够理解的格式。

一个很常见的问题是，模型的文件格式有很多种，每一种都会以它们自己的方式来导出模型数据。

* 像是[Wavefront的.obj](http://en.wikipedia.org/wiki/Wavefront_.obj_file)这样的模型格式，只包含了模型数据以及材质信息，像是模型颜色和漫反射/镜面光贴图。

* 而以XML为基础的[Collada文件格式](http://en.wikipedia.org/wiki/COLLADA)则非常的丰富，包含模型、光照、多种材质、动画数据、摄像机、完整的场景信息等等。Wavefront的.obj格式通常被认为是一个易于解析的模型格式。

建议至少去Wavefront的wiki页面上看看文件格式的信息是如何封装的。这应该能让你认识到模型文件的基本结构。

不同种类的文件格式有很多，它们之间通常并没有一个通用的结构。所以如果我们想从这些文件格式中导入模型的话，我们必须要去自己对每一种需要导入的文件格式写一个导入器。很幸运的是，正好有一个库专门处理这个问题。

## 模型加载库

一个非常流行的模型导入库是[Assimp](http://assimp.org/)，它是**Open Asset Import Library**（开放的资产导入库）的缩写。

当使用Assimp导入一个模型的时候，它通常会将整个模型加载进一个**场景**(Scene)对象，它会包含导入的模型/场景中的所有数据。Assimp会将场景载入为一系列的节点(Node)，每个节点包含了场景对象中所储存数据的索引，每个节点都可以有任意数量的子节点。Assimp数据结构的（简化）模型如下：

![img](D:\Program\OpenGL\LearnOpenGL\模型加载01 Assimp.assets\assimp_structure.png)

- 

**网格**

当使用建模工具对物体建模的时候，艺术家通常不会用单个形状创建出整个模型。通常每个模型都由几个子模型/形状组合而成。组合模型的每个单独的形状就叫做一个网格(Mesh)。比如说有一个人形的角色：艺术家通常会将头部、四肢、衣服、武器建模为分开的组件，并将这些网格组合而成的结果表现为最终的模型。一个网格是我们在OpenGL中绘制物体所需的最小单位（顶点数据、索引和材质属性）。一个模型（通常）会包括多个网格。

# 02 网格

* C++结构体有一个很棒的特性，它们的内存布局是连续的(Sequential)
* 预处理指令`offsetof(s, m)`，它的第一个参数是一个结构体，第二个参数是这个结构体中变量的名字。这个宏会返回那个变量距结构体头部的字节偏移量(Byte Offset)。这正好可以用在定义glVertexAttribPointer函数中的偏移参数

# 03 模型

* Assimp很棒的一点在于，它抽象掉了加载不同文件格式的所有技术细节，只需要一行代码就能完成所有的工作：

```c++
Assimp::Importer importer;
const aiScene *scene = importer.ReadFile(path, aiProcess_Triangulate | aiProcess_FlipUVs);
```

一些比较有用的选项有：

- aiProcess_GenNormals：如果模型不包含法向量的话，就为每个顶点创建法线。
- aiProcess_SplitLargeMeshes：将比较大的网格分割成更小的子网格，如果你的渲染有最大顶点数限制，只能渲染较小的网格，那么它会非常有用。
- aiProcess_OptimizeMeshes：和上个选项相反，它会将多个小网格拼接为一个大的网格，减少绘制调用从而进行优化。

Assimp提供了很多有用的后期处理指令，你可以在[这里](http://assimp.sourceforge.net/lib_html/postprocess_8h.html)找到全部的指令。

# 重大优化

逆天啊

用笔记本一直没窗口弹出来 ，还以为有bug

结果放在一边过了四五分钟才渲染出来，

![image-20230816194407180](D:\Program\OpenGL\LearnOpenGL\模型加载01 Assimp.assets\image-20230816194407180.png)

* 加个之前的小正方体
* ![image-20230816201405307](D:\Program\OpenGL\LearnOpenGL\模型加载01 Assimp.assets\image-20230816201405307.png)



