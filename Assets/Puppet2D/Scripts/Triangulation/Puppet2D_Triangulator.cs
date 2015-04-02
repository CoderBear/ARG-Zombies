/*using UnityEngine;
using System.Collections.Generic;

public class Triangulator
{
    private List<Vector2> m_points = new List<Vector2>();

    public Triangulator (Vector2[] points) {
        m_points = new List<Vector2>(points);
    }

    public int[] Triangulate() {
        List<int> indices = new List<int>();

        int n = m_points.Count;
        if (n < 3)
            return indices.ToArray();

        int[] V = new int[n];
        if (Area() > 0) {
            for (int v = 0; v < n; v++)
                V[v] = v;
        }
        else {
            for (int v = 0; v < n; v++)
                V[v] = (n - 1) - v;
        }

        int nv = n;
        int count = 2 * nv;
        for (int m = 0, v = nv - 1; nv > 2; ) {
            if ((count--) <= 0)
                return indices.ToArray();

            int u = v;
            if (nv <= u)
                u = 0;
            v = u + 1;
            if (nv <= v)
                v = 0;
            int w = v + 1;
            if (nv <= w)
                w = 0;

            if (Snip(u, v, w, nv, V)) {
                int a, b, c, s, t;
                a = V[u];
                b = V[v];
                c = V[w];
                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
                m++;
                for (s = v, t = v + 1; t < nv; s++, t++)
                    V[s] = V[t];
                nv--;
                count = 2 * nv;
            }
        }

        indices.Reverse();
        return indices.ToArray();
    }

    private float Area () {
        int n = m_points.Count;
        float A = 0.0f;
        for (int p = n - 1, q = 0; q < n; p = q++) {
            Vector2 pval = m_points[p];
            Vector2 qval = m_points[q];
            A += pval.x * qval.y - qval.x * pval.y;
        }
        return (A * 0.5f);
    }

    private bool Snip (int u, int v, int w, int n, int[] V) {
        int p;
        Vector2 A = m_points[V[u]];
        Vector2 B = m_points[V[v]];
        Vector2 C = m_points[V[w]];
        if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
            return false;
        for (p = 0; p < n; p++) {
            if ((p == u) || (p == v) || (p == w))
                continue;
            Vector2 P = m_points[V[p]];
            if (InsideTriangle(A, B, C, P))
                return false;
        }
        return true;
    }

    private bool InsideTriangle (Vector2 A, Vector2 B, Vector2 C, Vector2 P) {
        float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
        float cCROSSap, bCROSScp, aCROSSbp;

        ax = C.x - B.x; ay = C.y - B.y;
        bx = A.x - C.x; by = A.y - C.y;
        cx = B.x - A.x; cy = B.y - A.y;
        apx = P.x - A.x; apy = P.y - A.y;
        bpx = P.x - B.x; bpy = P.y - B.y;
        cpx = P.x - C.x; cpy = P.y - C.y;

        aCROSSbp = ax * bpy - ay * bpx;
        cCROSSap = cx * apy - cy * apx;
        bCROSScp = bx * cpy - by * cpx;

        return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
    }
}*/
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

struct Triangle {
    public int p1;
    public int p2;
    public int p3;
    public Triangle(int point1, int point2, int point3) {
        p1 = point1;
        p2 = point2;
        p3 = point3;
    }
}

class Edge {
    public int p1;
    public int p2;
    public Edge(int point1, int point2) {
        p1 = point1;
        p2 = point2;
    }
    public Edge(): this(0, 0) {}
    public bool Equals(Edge other) {
        return ((this.p1 == other.p2) && (this.p2 == other.p1)) || ((this.p1 == other.p1) && (this.p2 == other.p2));
    }
}


public class Puppet2D_Triangulator {
    
    bool TriangulatePolygonSubFunc_InCircle(Vector2 p, Vector2 p1, Vector2 p2, Vector2 p3) {
        if (Mathf.Abs(p1.y - p2.y) < float.Epsilon && Mathf.Abs(p2.y - p3.y) < float.Epsilon) {
            return false;
        }
        float m1, m2, mx1, mx2, my1, my2, xc, yc;
        if (Mathf.Abs(p2.y - p1.y) < float.Epsilon) {
            m2 = -(p3.x - p2.x) / (p3.y - p2.y);
            mx2 = (p2.x + p3.x) * 0.5f;
            my2 = (p2.y + p3.y) * 0.5f;
            xc = (p2.x + p1.x) * 0.5f;
            yc = m2 * (xc - mx2) + my2;
        } else if (Mathf.Abs(p3.y - p2.y) < float.Epsilon) {
            m1 = -(p2.x - p1.x) / (p2.y - p1.y);
            mx1 = (p1.x + p2.x) * 0.5f;
            my1 = (p1.y + p2.y) * 0.5f;
            xc = (p3.x + p2.x) * 0.5f;
            yc = m1 * (xc - mx1) + my1;
        } else {
            m1 = -(p2.x - p1.x) / (p2.y - p1.y);
            m2 = -(p3.x - p2.x) / (p3.y - p2.y);
            mx1 = (p1.x + p2.x) * 0.5f;
            mx2 = (p2.x + p3.x) * 0.5f;
            my1 = (p1.y + p2.y) * 0.5f;
            my2 = (p2.y + p3.y) * 0.5f;
            xc = (m1 * mx1 - m2 * mx2 + my2 - my1) / (m1 - m2);
            yc = m1 * (xc - mx1) + my1;
        }
        float dx = p2.x - xc;
        float dy = p2.y - yc;
        float rsqr = dx * dx + dy * dy;
        dx = p.x - xc;
        dy = p.y - yc;
        double drsqr = dx * dx + dy * dy;
        return (drsqr <= rsqr);
    }
    
    
    GameObject CreateInfluencePolygon(Vector2[] XZofVertices) {
        Vector3[] Vertices = new Vector3[XZofVertices.Length];
        for (int ii1 = 0; ii1 < XZofVertices.Length; ii1++) {
            Vertices[ii1] = new Vector3(XZofVertices[ii1].x, 0, XZofVertices[ii1].y);
        }
        GameObject OurNewMesh = new GameObject("OurNewMesh1");
        Mesh mesh = new Mesh();
        mesh.vertices = Vertices;
        mesh.uv = XZofVertices;
        mesh.triangles = TriangulatePolygon(XZofVertices);
        mesh.RecalculateNormals();
        MeshFilter mf = OurNewMesh.AddComponent < MeshFilter > ();
        OurNewMesh.AddComponent < MeshRenderer > ();
        //OurNewMesh.GetComponent().castShadows = false;
        //OurNewMesh.GetComponent().receiveShadows = false;
        mf.mesh = mesh;
        return OurNewMesh;
    }

    
    public int[] TriangulatePolygon(Vector2[] XZofVertices) {
        int VertexCount = XZofVertices.Length;
        float xmin = XZofVertices[0].x;
        float ymin = XZofVertices[0].y;
        float xmax = xmin;
        float ymax = ymin;
        for (int ii1 = 1; ii1 < VertexCount; ii1++) {
            if (XZofVertices[ii1].x < xmin) {
                xmin = XZofVertices[ii1].x;
            } else if (XZofVertices[ii1].x > xmax) {
                xmax = XZofVertices[ii1].x;
            }
            if (XZofVertices[ii1].y < ymin) {
                ymin = XZofVertices[ii1].y;
            } else if (XZofVertices[ii1].y > ymax) {
                ymax = XZofVertices[ii1].y;
            }
        }
        float dx = xmax - xmin;
        float dy = ymax - ymin;
        float dmax = (dx > dy) ? dx : dy;
        float xmid = (xmax + xmin) * 0.5f;
        float ymid = (ymax + ymin) * 0.5f;
        Vector2[] ExpandedXZ = new Vector2[3 + VertexCount];
        for (int ii1 = 0; ii1 < VertexCount; ii1++) {
            ExpandedXZ[ii1] = XZofVertices[ii1];
        }
        ExpandedXZ[VertexCount] = new Vector2((xmid - 2 * dmax), (ymid - dmax));
        ExpandedXZ[VertexCount + 1] = new Vector2(xmid, (ymid + 2 * dmax));
        ExpandedXZ[VertexCount + 2] = new Vector2((xmid + 2 * dmax), (ymid - dmax));
        List < Triangle > TriangleList = new List < Triangle > ();
        TriangleList.Add(new Triangle(VertexCount, VertexCount + 1, VertexCount + 2));
        for (int ii1 = 0; ii1 < VertexCount; ii1++) {
            List < Edge > Edges = new List < Edge > ();
            for (int ii2 = 0; ii2 < TriangleList.Count; ii2++) {
                if (TriangulatePolygonSubFunc_InCircle(ExpandedXZ[ii1], ExpandedXZ[TriangleList[ii2].p1], ExpandedXZ[TriangleList[ii2].p2], ExpandedXZ[TriangleList[ii2].p3])) {
                    Edges.Add(new Edge(TriangleList[ii2].p1, TriangleList[ii2].p2));
                    Edges.Add(new Edge(TriangleList[ii2].p2, TriangleList[ii2].p3));
                    Edges.Add(new Edge(TriangleList[ii2].p3, TriangleList[ii2].p1));
                    TriangleList.RemoveAt(ii2);
                    ii2--;
                }
            }
            if (ii1 >= VertexCount) {
                continue;
            }
            for (int ii2 = Edges.Count - 2; ii2 >= 0; ii2--) {
                for (int ii3 = Edges.Count - 1; ii3 >= ii2 + 1; ii3--) {
                    if (Edges[ii2].Equals(Edges[ii3])) {
                        Edges.RemoveAt(ii3);
                        Edges.RemoveAt(ii2);
                        ii3--;
                        continue;
                    }
                }
            }
            for (int ii2 = 0; ii2 < Edges.Count; ii2++) {
                TriangleList.Add(new Triangle(Edges[ii2].p1, Edges[ii2].p2, ii1));
            }
            Edges.Clear();
            Edges = null;
        }
        for (int ii1 = TriangleList.Count - 1; ii1 >= 0; ii1--) {
            if (TriangleList[ii1].p1 >= VertexCount || TriangleList[ii1].p2 >= VertexCount || TriangleList[ii1].p3 >= VertexCount) {
                TriangleList.RemoveAt(ii1);
            }
        }
        TriangleList.TrimExcess();
        int[] Triangles = new int[3 * TriangleList.Count];
        for (int ii1 = 0; ii1 < TriangleList.Count; ii1++) {
            Triangles[3 * ii1] = TriangleList[ii1].p1;
            Triangles[3 * ii1 + 1] = TriangleList[ii1].p2;
            Triangles[3 * ii1 + 2] = TriangleList[ii1].p3;
        }
        return Triangles;
    }
    
}