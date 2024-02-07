#version 330

uniform sampler2D inputTexture;
uniform vec2 offset;
uniform float height;
uniform float width;
uniform vec4 tint;
uniform int depth; // 0 - 29

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

void main() {
	vec4 newColor = texture(inputTexture, vec2(fragTexCoord.x * width + offset.x, fragTexCoord.y * height + offset.y));

	if (newColor.r == 1.0 && newColor.g == 1.0 && newColor.b == 1.0) discard;

	if (newColor.g == 1.0) {
		newColor = vec4(tint.r - 0.4, tint.g - 0.4, tint.b - 0.4, tint.a);
	}
	else if (newColor.r == 1.0) {
		newColor = vec4(tint.r - 0.6, tint.g - 0.6, tint.b - 0.6, tint.a);
	}
	else if (newColor.b == 1.0) {
		newColor = vec4(tint.r + 0.05, tint.g + 0.05, tint.b + 0.05, tint.a);
	}

	float depthWhite = depth * 0.03;

	newColor = vec4(newColor.r + depthWhite, newColor.g + depthWhite, newColor.b + depthWhite, newColor.a);

	if (newColor.a == 0.0) {
		discard;
	}

	FragColor = newColor;
}