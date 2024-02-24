#version 330 core
out vec4 FragColor;

in vec3 TextureDir;

uniform samplerCube skyBox;

void main()
{
	FragColor = texture(skyBox, TextureDir);
}