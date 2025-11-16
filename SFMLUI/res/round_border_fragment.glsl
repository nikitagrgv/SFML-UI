uniform vec2 u_size;
uniform vec4 u_radius;
uniform vec4 u_color;
uniform float u_border_width;

float sdRoundedBox(in vec2 p, in vec2 b, in vec4 r)
{
	r.xy = (p.x > 0.0) ? r.xy : r.zw;
	r.x  = (p.y > 0.0) ? r.x  : r.y;
	vec2 q = abs(p) - b + r.x;
	return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r.x;
}

void main()
{
	float x = (gl_TexCoord[0].x - 0.5) * u_size.x;
	float y = (gl_TexCoord[0].y - 0.5) * u_size.y;
	vec2 center = vec2(x, y);
	vec2 size = vec2(0.5 * u_size.x, 0.5 * u_size.y);
	float v = sdRoundedBox(center, size, u_radius);

	if (v < -u_border_width)
		discard;
    else
        gl_FragColor = u_color;
}