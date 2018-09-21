using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/GradientThreeColor")]
[RequireComponent(typeof(Text))]
[DisallowMultipleComponent]
public class GradientThreeColor : BaseMeshEffect {
    public Color ColorTop = Color.red;
    public Color ColorCenter = Color.blue;
    public Color ColorBottom = Color.green;

    public bool MultiplyTextColor = false;

    protected GradientThreeColor() {

    }

    public override void ModifyMesh(VertexHelper vh) {
        if (!this.IsActive()) {
            return;
        }

        ModifyVertices(vh);
    }

    private void ModifyVertices(VertexHelper vh) {
        List<UIVertex> verts = new List<UIVertex>(vh.currentVertCount);
        vh.GetUIVertexStream(verts);
        vh.Clear();

        int step = 6;

        for (int i = 0; i < verts.Count; i += step) {
            //6 point  
            var tl = MultiplyColor(verts[i + 0], ColorTop);
            var tr = MultiplyColor(verts[i + 1], ColorTop);
            var bl = MultiplyColor(verts[i + 4], ColorBottom);
            var br = MultiplyColor(verts[i + 3], ColorBottom);
            var cl = CalcCenterVertex(verts[i + 0], verts[i + 4]);
            var cr = CalcCenterVertex(verts[i + 1], verts[i + 2]);

            vh.AddVert(tl);
            vh.AddVert(tr);
            vh.AddVert(cr);
            vh.AddVert(cr);
            vh.AddVert(cl);
            vh.AddVert(tl);

            vh.AddVert(cl);
            vh.AddVert(cr);
            vh.AddVert(br);
            vh.AddVert(br);
            vh.AddVert(bl);
            vh.AddVert(cl);
        }

        for (int i = 0; i < vh.currentVertCount; i += 12) {
            vh.AddTriangle(i + 0, i + 1, i + 2);
            vh.AddTriangle(i + 3, i + 4, i + 5);
            vh.AddTriangle(i + 6, i + 7, i + 8);
            vh.AddTriangle(i + 9, i + 10, i + 11);
        }
    }

    private UIVertex MultiplyColor(UIVertex vertex, Color color) {
        if (MultiplyTextColor)
            vertex.color = Multiply(vertex.color, color);
        else
            vertex.color = color;
        return vertex;
    }

    private UIVertex CalcCenterVertex(UIVertex top, UIVertex bottom) {
        UIVertex center = new UIVertex();
        center.normal = (top.normal + bottom.normal) / 2;
        center.position = (top.position + bottom.position) / 2;
        center.tangent = (top.tangent + bottom.tangent) / 2;
        center.uv0 = (top.uv0 + bottom.uv0) / 2;
        center.uv1 = (top.uv1 + bottom.uv1) / 2;

        if (MultiplyTextColor) {
            //multiply color  
            var color = Color.Lerp(top.color, bottom.color, 0.5f);
            center.color = Multiply(color, ColorCenter);
        } else {
            center.color = ColorCenter;
        }

        return center;
    }

    public static Color32 Multiply(Color32 a, Color32 b) {
        a.r = (byte)((a.r * b.r) >> 8);
        a.g = (byte)((a.g * b.g) >> 8);
        a.b = (byte)((a.b * b.b) >> 8);
        a.a = (byte)((a.a * b.a) >> 8);
        return a;
    }
}