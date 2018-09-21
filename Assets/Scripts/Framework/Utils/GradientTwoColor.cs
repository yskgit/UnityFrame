using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

[AddComponentMenu("UI/Effects/GradientTwoColor")]
[RequireComponent(typeof(Text))]
[DisallowMultipleComponent]
public class GradientTwoColor : BaseMeshEffect {

    public Color ColorTop = Color.red;
    public Color ColorBottom = Color.green;

    protected GradientTwoColor() {

    }

    public override void ModifyMesh(VertexHelper vh) {
        if (!this.IsActive()) {
            return;
        }
        List<UIVertex> verts = new List<UIVertex>(vh.currentVertCount);
        vh.GetUIVertexStream(verts);

        ModifyVertices(verts);

        vh.Clear();
        vh.AddUIVertexTriangleStream(verts);
    }

    private void ModifyVertices(List<UIVertex> verts) {
        for (int i = 0; i < verts.Count; i += 6) {
            SetColor(verts, i + 0, ColorTop);
            SetColor(verts, i + 1, ColorTop);
            SetColor(verts, i + 2, ColorBottom);
            SetColor(verts, i + 3, ColorBottom);

            SetColor(verts, i + 4, ColorBottom);
            SetColor(verts, i + 5, ColorTop);
        }
    }

    private static void SetColor(List<UIVertex> verts, int index, Color32 c) {
        UIVertex vertex = verts[index];
        vertex.color = c;
        verts[index] = vertex;
    }
}