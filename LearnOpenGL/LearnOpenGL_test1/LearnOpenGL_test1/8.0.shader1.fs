#version 330 core
out vec4 FragColor;

uniform vec3 objectColor;
uniform vec3 lightColor;
uniform float alpha;

void main()
{

    FragColor = vec4(objectColor * lightColor,1.0f);

}