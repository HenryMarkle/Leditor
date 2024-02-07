#version 330

uniform sampler2D inputTexture;
uniform int layerNum;
uniform float layerHeight;
uniform float layerWidth;
uniform vec4 tint;
uniform int depth; // 0 - 29

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

void main() {
	vec4 newColor = vec4(0);
	float totalWidth = fragTexCoord.x * layerWidth;

	for (int l = layerNum - 1; l > -1; l--) {
		float currentHeight = fragTexCoord.y * layerHeight + (l * layerHeight);
		
		vec2 newFragTexCoord = vec2(totalWidth, currentHeight);
	
		vec4 c = texture(inputTexture, newFragTexCoord);
		if (c.r == 1.0 && c.g == 1.0 && c.b == 1.0) continue;
		
		if (c.g == 1.0) {
			newColor = vec4(tint.r - 0.3, tint.g - 0.3, tint.b - 0.3, tint.a);
		}
		else if (c.r == 1.0) {
			newColor = vec4(tint.r - 0.6, tint.g - 0.6, tint.b - 0.6, tint.a);
		}
		else if (c.b == 1.0) {
			newColor = vec4(tint.r - 0.1, tint.g - 0.1, tint.b - 0.1, tint.a);
		}
		
		float depthWhite = (depth + l) * 0.03;
		
		newColor = vec4(newColor.r + depthWhite, newColor.g + depthWhite, newColor.b + depthWhite, newColor.a);
	}

	if (newColor.a == 0.0) {
		discard;
	}

	FragColor = newColor;
}