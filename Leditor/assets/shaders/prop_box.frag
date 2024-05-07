#version 330

uniform sampler2D inputTexture;
uniform vec2 offset;
uniform float height;
uniform float width;
uniform int depth; // 0 - 29

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

void main() {
	vec4 newColor = texture(inputTexture, vec2(fragTexCoord.x * width + offset.x, fragTexCoord.y * height + offset.y));

	if (newColor.r == 1.0 && newColor.g == 1.0 && newColor.b == 1.0) discard;

	float depthWhite = depth * 0.03;

	newColor = vec4(newColor.r + depthWhite, newColor.g + depthWhite, newColor.b + depthWhite, fragColor.a);

	if (newColor.a == 0.0) {
		discard;
	}

	FragColor = newColor;
}