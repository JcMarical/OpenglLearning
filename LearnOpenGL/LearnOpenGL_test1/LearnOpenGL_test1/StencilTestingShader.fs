#version 330 core
out vec4 FragColor;

uniform vec3 objectColor;
uniform vec3 lightColor;
uniform float alpha;

float near = 0.1;
float far = 100.0;

float LinearizeDepth(float depth)
{
    float z = depth * 2.0 - 1.0; // back To NDC
    return (2.0 * near * far) / (far + near - z * (far - near));  
}

void main()
{
    
    FragColor = vec4(0.04, 0.28, 0.26, 1.0);

}