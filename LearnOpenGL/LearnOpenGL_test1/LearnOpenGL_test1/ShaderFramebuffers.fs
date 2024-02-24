#version 330 core

in vec2 TexCoords;

out vec4 FragColor;

uniform sampler2D screenTexture;


//kernel effect
const float offset = 1.0 / 300.0;

void main()
{
	//Default
	//-------
	//FragColor = texture(screenTexture, TexCoords);
	
	//Inversion
	//---------
	//FragColor = vec4(vec3(1.0-texture(screenTexture, TexCoords)), 1.0);

	//Grayscale
	//---------
	//FragColor = texture(screenTexture, TexCoords);
	//float average = 0.2126 * FragColor.r + 0.7152 * FragColor.g + 0.0722 * FragColor.b;
	//FragColor = vec4(average,average,average, 1.0);

	//Kernel Effect
	//-------------
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

	//sharpen
	//float kernel[9] = float[](
	//	-1, -1, -1,
	//	-1,  9, -1,
	//	-1, -1, -1
	//);

	//Blur
	//float kernel[9] = float[](
	//	1.0 / 16, 2.0 / 16, 1.0 / 16,
	//	2.0 / 16, 4.0 / 16, 2.0 / 16,
	//	1.0 / 16, 2.0 / 16, 1.0 / 16
	//);

	//Edge Detection
	float kernel[9] = float[](
		1,  1, 1,
		1, -8, 1,
		1,  1, 1
	);
	vec3 samplerTex[9];
	for(int i = 0; i < 9; i++)
	{
		samplerTex[i] = vec3(texture(screenTexture, TexCoords.st + offsets[i]));
	}

	vec3 col = vec3(0.0);

	for(int i = 0; i < 9; i++)
	{
		col += samplerTex[i] * kernel[i];
	}

	FragColor = vec4(col, 1.0);
}