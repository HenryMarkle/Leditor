#version 330

uniform sampler2D inputTexture;
uniform int depth;

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

void main() {
    float depthTint = 0.03 * depth;
    vec4 c = texture(inputTexture, fragTexCoord);

    if ((c.r == 1.0 && c.g == 1.0 && c.b == 1.0)) {
        discard;
    }

    FragColor = vec4(c.r + depthTint, c.g + depthTint, c.b + depthTint, c.a);
}
