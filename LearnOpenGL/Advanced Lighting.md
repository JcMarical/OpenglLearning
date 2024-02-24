# 01 Adavanced Lighting高级光照

## Phong

* specular area -- cut off
  * reason: the angle between view and  reflection  vector doesn't go over 90 degrees;
  * ![Result of Phong specular reflection with low exponent](D:\Program\OpenGL\LearnOpenGL\Advanced Lighting.assets\advanced_lighting_phong_limit.png)

![Image of Phong's reflection vectors being incorrect when larger than 90 degrees](D:\Program\OpenGL\LearnOpenGL\Advanced Lighting.assets\advanced_lighting_over_90.png)

# Blinn Phong

* use **halfway vector**
*  The closer this halfway vector aligns with the surface's normal vector, the higher the specular contribution.

![image-20230912103130497](D:\Program\OpenGL\LearnOpenGL\Advanced Lighting.assets\image-20230912103130497.png)

![Illustration of Blinn-Phong's halfway vector](D:\Program\OpenGL\LearnOpenGL\Advanced Lighting.assets\advanced_lighting_halfway_vector.png)

# 02 Gamma Correction

### Resolution 1

Enabling GL_FRAMEBUFFER_SRGB is as simple as calling glEnable:

```
glEnable(GL_FRAMEBUFFER_SRGB); 
```

### Resolution 2

Fragment ShaderEnabling GL_FRAMEBUFFER_SRGB is as simple as calling glEnable:

```
glEnable(GL_FRAMEBUFFER_SRGB); 
```

## sRGB textures

The texture image is way too bright and this happens because it is actually gamma corrected twice! 

![Comparrison between working in linear space with sRGB textures and linear-space textures](D:\Program\OpenGL\LearnOpenGL\Advanced Lighting.assets\gamma_correction_srgbtextures.png)

### solution 1

we have to make sure texture artists work in linear space. 



### solution 2

The other solution is to re-correct or transform these sRGB textures to linear space before doing any calculations on their color values. We can do this as follows:

```
float gamma = 2.2;
vec3 diffuseColor = pow(texture(diffuse, texCoords).rgb, vec3(gamma));
```



To do this for each texture in sRGB space is quite troublesome though. Luckily OpenGL gives us yet another solution to our problems by giving us the GL_SRGB and GL_SRGB_ALPHA internal texture formats:

```
glTexImage2D(GL_TEXTURE_2D, 0, GL_SRGB, width, height, 0, GL_RGB, GL_UNSIGNED_BYTE, data); 
```

## Attenuation

In the real physical world, lighting attenuates closely inversely proportional to the squared distance from a light source:

```
float attenuation = 1.0 / (distance * distance);
```

#  Gamma Correction

As soon as we compute the final pixel colors of the scene we will have to display them on a monitor. In the old days of digital imaging most monitors were cathode-ray tube (CRT) monitors. These monitors had the physical property that twice the input voltage did not result in twice the amount of brightness. Doubling the input voltage resulted in a brightness equal to an exponential relationship of roughly 2.2 known as the gamma of a monitor. This happens to (coincidently) also closely match how human beings measure brightness as brightness is also displayed with a similar (inverse) power relationship. To better understand what this all means take a look at the following image:

![Linear encodings of display with and without gamma correction](D:\Program\OpenGL\LearnOpenGL\Advanced Lighting.assets\gamma_correction_brightness.png)

The top line looks like the correct brightness scale to the human eye, doubling the brightness (from 0.1 to 0.2 for example) does indeed look like it's twice as bright with nice consistent differences. However, when we're talking about the physical brightness of light e.g. amount of photons leaving a light source, the bottom scale actually displays the correct brightness. At the bottom scale, doubling the brightness returns the correct physical brightness, but since our eyes perceive brightness differently (more susceptible to changes in dark colors) it looks weird.

Because the human eyes prefer to see brightness colors according to the top scale, monitors (still today) use a power relationship for displaying output colors so that the original physical brightness colors are mapped to the non-linear brightness colors in the top scale.

This non-linear mapping of monitors does output more pleasing brightness results for our eyes, but when it comes to rendering graphics there is one issue: all the color and brightness options we configure in our applications are based on what we perceive from the monitor and thus all the options are actually non-linear brightness/color options. Take a look at the graph below:

![Gamme curves](https://learnopengl.com/img/advanced-lighting/gamma_correction_gamma_curves.png)

The dotted line represents color/light values in linear space and the solid line represents the color space that monitors display. If we double a color in linear space, its result is indeed double the value. For instance, take a light's color vector (0.5, 0.0, 0.0) which represents a semi-dark red light. If we would double this light in linear space it would become (1.0, 0.0, 0.0) as you can see in the graph. However, the original color gets displayed on the monitor as (0.218, 0.0, 0.0) as you can see from the graph. Here's where the issues start to rise: once we double the dark-red light in linear space, it actually becomes more than 4.5 times as bright on the monitor!

Up until this chapter we have assumed we were working in linear space, but we've actually been working in the monitor's output space so all colors and lighting variables we configured weren't physically correct, but merely looked (sort of) right on our monitor. For this reason, we (and artists) generally set lighting values way brighter than they should be (since the monitor darkens them) which as a result makes most linear-space calculations incorrect. Note that the monitor (CRT) and linear graph both start and end at the same position; it is the intermediate values that are darkened by the display.

Because colors are configured based on the display's output, all intermediate (lighting) calculations in linear-space are physically incorrect. This becomes more obvious as more advanced lighting algorithms are in place, as you can see in the image below:

![Example of gamma correction w/ and without on advanced rendering](D:\Program\OpenGL\LearnOpenGL\Advanced Lighting.assets\gamma_correction_example.png)

You can see that with gamma correction, the (updated) color values work more nicely together and darker areas show more details. Overall, a better image quality with a few small modifications.

Without properly correcting this monitor gamma, the lighting looks wrong and artists will have a hard time getting realistic and good-looking results. The solution is to apply gamma correction.

## Gamma correction

The idea of gamma correction is to apply the inverse of the monitor's gamma to the final output color before displaying to the monitor. Looking back at the gamma curve graph earlier this chapter we see another *dashed* line that is the inverse of the monitor's gamma curve. We multiply each of the linear output colors by this inverse gamma curve (making them brighter) and as soon as the colors are displayed on the monitor, the monitor's gamma curve is applied and the resulting colors become linear. We effectively brighten the intermediate colors so that as soon as the monitor darkens them, it balances all out.

Let's give another example. Say we again have the dark-red color (0.5,0.0,0.0)(0.5,0.0,0.0). Before displaying this color to the monitor we first apply the gamma correction curve to the color value. Linear colors displayed by a monitor are roughly scaled to a power of 2.22.2 so the inverse requires scaling the colors by a power of 1/2.21/2.2. The gamma-corrected dark-red color thus becomes (0.5,0.0,0.0)1/2.2=(0.5,0.0,0.0)0.45=(0.73,0.0,0.0)(0.5,0.0,0.0)1/2.2=(0.5,0.0,0.0)0.45=(0.73,0.0,0.0). The corrected colors are then fed to the monitor and as a result the color is displayed as (0.73,0.0,0.0)2.2=(0.5,0.0,0.0)(0.73,0.0,0.0)2.2=(0.5,0.0,0.0). You can see that by using gamma-correction, the monitor now finally displays the colors as we linearly set them in the application.

A gamma value of 2.2 is a default gamma value that roughly estimates the average gamma of most displays. The color space as a result of this gamma of 2.2 is called the sRGB color space (not 100% exact, but close). Each monitor has their own gamma curves, but a gamma value of 2.2 gives good results on most monitors. For this reason, games often allow players to change the game's gamma setting as it varies slightly per monitor.

There are two ways to apply gamma correction to your scene:

- By using OpenGL's built-in sRGB framebuffer support.
- By doing the gamma correction ourselves in the fragment shader(s).

The first option is probably the easiest, but also gives you less control. By enabling GL_FRAMEBUFFER_SRGB you tell OpenGL that each subsequent drawing command should first gamma correct colors (from the sRGB color space) before storing them in color buffer(s). The sRGB is a color space that roughly corresponds to a gamma of 2.2 and a standard for most devices. After enabling GL_FRAMEBUFFER_SRGB, OpenGL automatically performs gamma correction after each fragment shader run to all subsequent framebuffers, including the default framebuffer.

Enabling GL_FRAMEBUFFER_SRGB is as simple as calling glEnable:

```
glEnable(GL_FRAMEBUFFER_SRGB); 
```

From now on your rendered images will be gamma corrected and as this is done by the hardware it is completely free. Something you should keep in mind with this approach (and the other approach) is that gamma correction (also) transforms the colors from linear space to non-linear space so it is very important you only do gamma correction at the last and final step. If you gamma-correct your colors before the final output, all subsequent operations on those colors will operate on incorrect values. For instance, if you use multiple framebuffers you probably want intermediate results passed in between framebuffers to remain in linear-space and only have the last framebuffer apply gamma correction before being sent to the monitor.

The second approach requires a bit more work, but also gives us complete control over the gamma operations. We apply gamma correction at the end of each relevant fragment shader run so the final colors end up gamma corrected before being sent out to the monitor:

```
void main()
{
    // do super fancy lighting in linear space
    [...]
    // apply gamma correction
    float gamma = 2.2;
    FragColor.rgb = pow(fragColor.rgb, vec3(1.0/gamma));
}
```

The last line of code effectively raises each individual color component of fragColor to `1.0/gamma`, correcting the output color of this fragment shader run.

An issue with this approach is that in order to be consistent you have to apply gamma correction to each fragment shader that contributes to the final output. If you have a dozen fragment shaders for multiple objects, you have to add the gamma correction code to each of these shaders. An easier solution would be to introduce a post-processing stage in your render loop and apply gamma correction on the post-processed quad as a final step which you'd only have to do once.

That one line represents the technical implementation of gamma correction. Not all too impressive, but there are a few extra things you have to consider when doing gamma correction.

## sRGB textures

Because monitors display colors with gamma applied, whenever you draw, edit, or paint a picture on your computer you are picking colors based on what you see on the monitor. This effectively means all the pictures you create or edit are not in linear space, but in sRGB space e.g. doubling a dark-red color on your screen based on perceived brightness, does not equal double the red component.

As a result, when texture artists create art by eye, all the textures' values are in sRGB space so if we use those textures as they are in our rendering application we have to take this into account. Before we knew about gamma correction this wasn't really an issue, because the textures looked good in sRGB space which is the same space we worked in; the textures were displayed exactly as they are which was fine. However, now that we're displaying everything in linear space, the texture colors will be off as the following image shows:

![Comparrison between working in linear space with sRGB textures and linear-space textures](D:\Program\OpenGL\LearnOpenGL\Advanced Lighting.assets\gamma_correction_srgbtextures-1694694963591-6.png)

The texture image is way too bright and this happens because it is actually gamma corrected twice! Think about it, when we create an image based on what we see on the monitor, we effectively gamma correct the color values of an image so that it looks right on the monitor. Because we then again gamma correct in the renderer, the image ends up way too bright.

To fix this issue we have to make sure texture artists work in linear space. However, since it's easier to work in sRGB space and most tools don't even properly support linear texturing, this is probably not the preferred solution.

The other solution is to re-correct or transform these sRGB textures to linear space before doing any calculations on their color values. We can do this as follows:

```
float gamma = 2.2;
vec3 diffuseColor = pow(texture(diffuse, texCoords).rgb, vec3(gamma));
```

To do this for each texture in sRGB space is quite troublesome though. Luckily OpenGL gives us yet another solution to our problems by giving us the GL_SRGB and GL_SRGB_ALPHA internal texture formats.

If we create a texture in OpenGL with any of these two sRGB texture formats, OpenGL will automatically correct the colors to linear-space as soon as we use them, allowing us to properly work in linear space. We can specify a texture as an sRGB texture as follows:

```
glTexImage2D(GL_TEXTURE_2D, 0, GL_SRGB, width, height, 0, GL_RGB, GL_UNSIGNED_BYTE, data);  
```

If you also want to include alpha components in your texture you'll have to specify the texture's internal format as GL_SRGB_ALPHA.

You should be careful when specifying your textures in sRGB space as not all textures will actually be in sRGB space. Textures used for coloring objects (like diffuse textures) are almost always in sRGB space. Textures used for retrieving lighting parameters (like [specular maps](https://learnopengl.com/Lighting/Lighting-maps) and [normal maps](https://learnopengl.com/Advanced-Lighting/Normal-Mapping)) are almost always in linear space, so if you were to configure these as sRGB textures the lighting will look odd. Be careful in which textures you specify as sRGB.

With our diffuse textures specified as sRGB textures you get the visual output you'd expect again, but this time everything is gamma corrected only once.

## Attenuation

Something else that's different with gamma correction is lighting attenuation. In the real physical world, lighting attenuates closely inversely proportional to the squared distance from a light source. In normal English it simply means that the light strength is reduced over the distance to the light source squared, like below:

```
float attenuation = 1.0 / (distance * distance);
```

However, when using this equation the attenuation effect is usually way too strong, giving lights a small radius that doesn't look physically right. For that reason other attenuation functions were used:

```
float attenuation = 1.0 / distance;  
```

### Why？

the linear equivalent makes much more sense without gamma correction as this effectively becomes 

![image-20230914203718368](D:\Program\OpenGL\LearnOpenGL\Advanced Lighting.assets\image-20230914203718368.png)

 which resembles its physical equivalent a lot more.

# 03 Shadow Mapping

Iterating through possibly thousands of light rays from such a light source is an extremely inefficient approach and doesn't lend itself too well for real-time rendering. 

we use something we're quite familiar with: **the depth buffer**.

What if we were to render the scene **from the light's perspective** and **store the resulting depth values in a texture**? 

![Different coordinate transforms / spaces for shadow mapping.](D:\Program\OpenGL\LearnOpenGL\Advanced Lighting.assets\shadow_mapping_theory_spaces.png)

## The Depth Map 深度贴图

First we'll create a framebuffer object for rendering the depth map:

```
unsigned int depthMapFBO;
glGenFramebuffers(1, &depthMapFBO);  
```

Next we create a 2D texture that we'll use as the framebuffer's depth buffer:

```
const unsigned int SHADOW_WIDTH = 1024, SHADOW_HEIGHT = 1024;

unsigned int depthMap;
glGenTextures(1, &depthMap);
glBindTexture(GL_TEXTURE_2D, depthMap);
glTexImage2D(GL_TEXTURE_2D, 0, GL_DEPTH_COMPONENT, 
             SHADOW_WIDTH, SHADOW_HEIGHT, 0, GL_DEPTH_COMPONENT, GL_FLOAT, NULL);
glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT); 
```

### Light space transform 光源空间变换



# 06 HDR

## Tone Mapping 色调映射

### ReinHard

 It does tend to slightly favor brighter areas, making darker regions seem less detailed and distinct:

```
  // Reinhard色调映射
    vec3 mapped = hdrColor / (hdrColor + vec3(1.0));
```



### Exposure曝光色调映射

在这里我们将`exposure`定义为默认为1.0的`uniform`，从而允许我们更加精确设定我们是要注重黑暗还是明亮的区域的HDR颜色值。举例来说，高曝光值会使隧道的黑暗部分显示更多的细节，然而低曝光值会显著减少黑暗区域的细节，但允许我们看到更多明亮区域的细节。下面这组图片展示了在不同曝光值下的通道：

```
uniform float exposure;

void main()
{             
    const float gamma = 2.2;
    vec3 hdrColor = texture(hdrBuffer, TexCoords).rgb;

    // 曝光色调映射
    vec3 mapped = vec3(1.0) - exp(-hdrColor * exposure);
    // Gamma校正 
    mapped = pow(mapped, vec3(1.0 / gamma));

    color = vec4(mapped, 1.0);
    }
```

