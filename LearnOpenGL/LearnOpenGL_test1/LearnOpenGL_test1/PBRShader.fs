 #version 330 core
 out vec4 FragColor;
 in vec2 TexCoords;
 in vec3 WorldPos;
 in vec3 Normal;

uniform vec3 camPos;

//material parameters
uniform vec3 albedo;
uniform float metallic;
uniform float roughness;
uniform float ao;

//lights
uniform vec3 lightPositions[4];
uniform vec3 lightColors[4];

const float PI = 3.14159265359;






vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
	return F0 + (1.0 - F0) * pow(clamp(1.0-cosTheta, 0.0 , 1.0), 5.0 );
}

float DistributionGGX(vec3 N, vec3 H, float roughness)
{
	float a = roughness;
	float a2 = a*a;
	float NDotH = dot(N, H);
	float NDotH2 = NDotH*NDotH;

	float numerator = a2;
	float denominator = (NDotH2*(a2-1.0) + 1.0);
	denominator = PI * denominator * denominator;

	return numerator/denominator;
}

float GeometrySchlickGGX(float NDotV, float k)
{
	float numerator = NDotV;
	float denominator = NDotV*(1-k) + k;

	return numerator/denominator;
}

float GeometrySmith(vec3 N, vec3 V, vec3 L, float k)
{
	float NDotV = max(dot(N,V),0.0);
	float NDotL = max(dot(N,L),0.0);

	float ggx1 = GeometrySchlickGGX(NDotV, k);
	float ggx2 = GeometrySchlickGGX(NDotL, k);

	return ggx1 * ggx2;
}

void main()
{
	vec3 N = normalize(Normal);
	vec3 V = normalize(camPos - WorldPos);

	vec3 Lo = vec3(0.0);
	for(int i = 0 ; i< 4 ;++i)
	{
		vec3 L = normalize(lightPositions[i] - WorldPos);
		vec3 H = normalize(V + L);

		float distance = length(lightPositions[i]-WorldPos);
		float attenuation = 1.0/(distance * distance);
		vec3 radiance = lightColors[i] * attenuation;

		//PBR
		//-------------------
		//fresnelSchlick
		vec3 F0 = vec3(0.04);
		F0 = mix(F0, albedo , metallic);
		vec3 F = fresnelSchlick(max(dot(H,V),0.0), F0);

		//DistributionGGX && GeometrySmith
		float NDF = DistributionGGX(N,H,roughness);
		float G  = GeometrySmith(N,V,L,roughness);

		//specular
		vec3 numerator = NDF * G * F;
		float denominator = 4.0 * max(dot(N , V ), 0.0) * max(dot(N, L), 00) + 0.001;
		
		vec3 specular =  numerator/ denominator;

		//specular && diffuse 
		vec3 KS = F;
		vec3 KD = vec3(1.0) - KS;


		KD *= 1.0 - metallic;

		//Light  Sum

		float NdotL = max(dot(N, L), 0.0);        
		Lo += (KD * albedo / PI + specular) * radiance * NdotL;
	}
		//ambient
		vec3 ambient = vec3(0.03) * albedo * ao;
		vec3 color   = ambient + Lo;  

		//HDR 
		color = color / (color + vec3(1.0));
		
		//Gamma Correction
		color = pow(color, vec3(1.0/2.2)); 

		FragColor = vec4(color,1.0f);
}