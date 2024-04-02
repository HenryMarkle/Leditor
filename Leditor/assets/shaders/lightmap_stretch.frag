#version 330

uniform sampler2D textureSampler;

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

void main() {
    vec2 texSize = textureSize(textureSampler, 0);
    vec2 uv = fragTexCoord * texSize;
    vec2 uv00 = floor(uv);
    vec2 uv11 = uv00 + 1.0;
    vec2 f = fract(uv);
    vec4 texColor00 = texture(textureSampler, uv00 / texSize);
    vec4 texColor10 = texture(textureSampler, vec2(uv11.x, uv00.y) / texSize);
    vec4 texColor01 = texture(textureSampler, vec2(uv00.x, uv11.y) / texSize);
    vec4 texColor11 = texture(textureSampler, uv11 / texSize);
    vec4 top = mix(texColor00, texColor10, f.x);
    vec4 bottom = mix(texColor01, texColor11, f.x);
    
    FragColor = mix(top, bottom, f.y);
}