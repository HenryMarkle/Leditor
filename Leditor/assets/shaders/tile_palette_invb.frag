#version 330

uniform sampler2D inputTexture;
uniform sampler2D paletteTexture;

uniform int layerNum;
uniform float layerHeight;
uniform float layerWidth;
uniform int depth; // 0 - 29
uniform float alpha; // 0 - 1

in vec2 fragTexCoord;
in vec4 fragColor;

uniform vec2 vertex_pos[4];

out vec4 FragColor;

float cross2d(vec2 a, vec2 b) {
	return a.x * b.y - a.y * b.x;
}

vec2 invbilinear( vec2 p, vec2 a, vec2 b, vec2 c, vec2 d )
{
    vec2 res = vec2(-1.0);

    vec2 e = b-a;
    vec2 f = d-a;
    vec2 g = a-b+c-d;
    vec2 h = p-a;
        
    float k2 = cross2d( g, f );
    float k1 = cross2d( e, f ) + cross2d( h, g );
    float k0 = cross2d( h, e );
    
    // if edges are parallel, this is a linear equation
    if( abs(k2)<0.001 )
    {
        res = vec2( (h.x*k1+f.x*k0)/(e.x*k1-g.x*k0), -k0/k1 );
    }
    // otherwise, it's a quadratic
    else
    {
        float w = k1*k1 - 4.0*k0*k2;
        if( w<0.0 ) return vec2(-1.0);
        w = sqrt( w );

        float ik2 = 0.5/k2;
        float v = (-k1 - w)*ik2;
        float u = (h.x - f.x*v)/(e.x + g.x*v);
        
        if( u<0.0 || u>1.0 || v<0.0 || v>1.0 )
        {
           v = (-k1 + w)*ik2;
           u = (h.x - f.x*v)/(e.x + g.x*v);
        }
        res = vec2( u, v );
    }
    
    return res;
}

void main() {
    vec2 a = vertex_pos[1]; // top left
	vec2 b = vertex_pos[0]; // top right
	vec2 c = vertex_pos[3]; // bottom right
	vec2 d = vertex_pos[2]; // bottom left

	vec2 p = fragTexCoord;
	
	vec2 uv = invbilinear(p, a, b, c, d);

	vec4 newColor = vec4(0);
	float totalWidth = uv.x * layerWidth;

	for (int l = layerNum - 1; l > -1; l--) {
		float currentHeight = uv.y * layerHeight + (l * layerHeight);
		
		vec2 newFragTexCoord = vec2(totalWidth, currentHeight);
	
		vec4 c = texture(inputTexture, newFragTexCoord);
		
        if (c.r == 1.0 && c.g == 1.0 && c.b == 1.0) continue;

        float depthCoord = (depth + l);

        if (depthCoord > 29.0) continue;

        if (c.b == 1.0) {
            newColor = texture(paletteTexture, vec2(depthCoord/32.0, 2.0/16.0));
        } 
        else if (c.g == 1.0) {
            newColor = texture(paletteTexture, vec2(depthCoord/32.0, 3.0/16.0));
        }
        else if (c.r == 1.0) {
            newColor = texture(paletteTexture, vec2(depthCoord/32.0, 4.0/16.0));
        }
	}

    if (newColor.a <= 0.0) discard;

    newColor.a = alpha;

	FragColor = newColor;
}