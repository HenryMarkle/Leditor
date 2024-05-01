#version 330

uniform sampler2D tilingTexture;
uniform sampler2D maskTexture;

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

void main() {
    vec4 c = texture(maskTexture, fragTexCoord);
    
    if (!(c.r == 0.0 && c.g == 0.0 && c.b == 0.0)) {
        discard;
    }

    FragColor = texture(tilingTexture, fragTexCoord);
}