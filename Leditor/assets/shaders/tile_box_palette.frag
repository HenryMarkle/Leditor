#version 330

uniform sampler2D inputTexture;
uniform sampler2D paletteTexture;

uniform vec2 offset;
uniform float height;
uniform float width;
uniform int depth; // 0 - 29
uniform float alpha; // 0 - 1
 
in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

void main() {
	vec4 newColor = texture(inputTexture, vec2(fragTexCoord.x * width + offset.x, fragTexCoord.y * height + offset.y));

	if (newColor.r == 1.0 && newColor.g == 1.0 && newColor.b == 1.0) discard;

	if (newColor.b == 1.0) {
		newColor = texture(paletteTexture, vec2(depth/32.0, 2.0/16.0));
	}
	else if (newColor.g == 1.0) {
		newColor = texture(paletteTexture, vec2(depth/32.0, 3.0/16.0));
	}
	else if (newColor.r == 1.0) {
		newColor = texture(paletteTexture, vec2(depth/32.0, 4.0/16.0));
	} else {
        discard;
    }

    newColor.a = alpha;

	if (newColor.a == 0.0) {
		discard;
	}

	FragColor = newColor;
}