using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCutter : MonoBehaviour
{
    public static GameObject[] Slicer(GameObject _target, Vector3 _sliceNormal, Vector3 _slicePoint, Material _ineterial)//자를 오브젝트, 단면의 수직벡터, 자르는 한 점
    {
        Mesh orinMesh = _target.GetComponent<MeshFilter>().sharedMesh;
        Vector3[] orinVerts = orinMesh.vertices;
        Vector3[] orinNors = orinMesh.normals;
        Vector2[] orinUvs = orinMesh.uv;

        //기존 정점들을 두 가지로 나누어 저장해둘 곳
        List<Vector3> aSideVerts = new List<Vector3>();
        List<Vector3> bSideVerts = new List<Vector3>();
        List<Vector3> aSideNors = new List<Vector3>();
        List<Vector3> bSideNors = new List<Vector3>();
        List<Vector2> aSideUvs = new List<Vector2>();
        List<Vector2> bSideUvs = new List<Vector2>();
        List<int> aSideTris = new List<int>();
        List<int> bSideTris = new List<int>();

        //새로 생성된 정점들에 대한 정보들이 저장될 곳
        List<Vector3> createdVerts = new List<Vector3>();
        List<Vector3> createdNors = new List<Vector3>();
        List<Vector2> createdUvs = new List<Vector2>();

        int triCount = orinMesh.triangles.Length / 3;//폴리곤 각각의 모든 정점들의 수를 3으로 나눠 폴리곤의 개수를 구한다.
        for (int i = 0; i < triCount; i++)//폴리곤의 수만큼 for문을 돌린다.
        {
            int idx0 = i * 3;
            int idx1 = idx0 + 1;
            int idx2 = idx1 + 1;
            int vertIdx0 = orinMesh.triangles[idx0];
            int vertIdx1 = orinMesh.triangles[idx1];
            int vertIdx2 = orinMesh.triangles[idx2];//삼각형을 그리는 순서대로 그려야 겹치지 않고 모두 확인할수 있다.
            Vector3 vert0 = orinVerts[vertIdx0];
            Vector3 vert1 = orinVerts[vertIdx1];
            Vector3 vert2 = orinVerts[vertIdx2];
            Vector3 nor0 = orinNors[vertIdx0];
            Vector3 nor1 = orinNors[vertIdx1];
            Vector3 nor2 = orinNors[vertIdx2];
            Vector2 uv0 = orinUvs[vertIdx0];
            Vector2 uv1 = orinUvs[vertIdx1];
            Vector2 uv2 = orinUvs[vertIdx2];

            //단면의 노멀벡터와 정점부터 잘려지는 곳까지의 벡터를 내적한다.
            float dot0 = Vector3.Dot(_sliceNormal, vert0 - _slicePoint);
            float dot1 = Vector3.Dot(_sliceNormal, vert1 - _slicePoint);
            float dot2 = Vector3.Dot(_sliceNormal, vert2 - _slicePoint);
            //이 과정을 통해 각 정점들이 면의 앞에 있는지 뒤에 있는지를 알수 있다.
            if (dot0 < 0 && dot1 < 0 && dot2 < 0)//모든 점들이 뒤에 있을 떄
            {
                aSideVerts.Add(vert0);
                aSideVerts.Add(vert1);
                aSideVerts.Add(vert2);
                aSideNors.Add(nor0);
                aSideNors.Add(nor1);
                aSideNors.Add(nor2);
                aSideUvs.Add(uv0);
                aSideUvs.Add(uv1);
                aSideUvs.Add(uv2);
                aSideTris.Add(aSideTris.Count);
                aSideTris.Add(aSideTris.Count);
                aSideTris.Add(aSideTris.Count);
            }
            else if (dot0 >= 0 && dot1 >= 0 && dot2 >= 0)//모든 점들이 앞에 있을때
            {
                bSideVerts.Add(vert0);
                bSideVerts.Add(vert1);
                bSideVerts.Add(vert2);
                bSideNors.Add(nor0);
                bSideNors.Add(nor1);
                bSideNors.Add(nor2);
                bSideUvs.Add(uv0);
                bSideUvs.Add(uv1);
                bSideUvs.Add(uv2);
                bSideTris.Add(bSideTris.Count);
                bSideTris.Add(bSideTris.Count);
                bSideTris.Add(bSideTris.Count);
            }//여기까지 기존에 있던 정점들을 내적을 통하여 Aside와 Bside로 나누는 작업(여기에 포함된 폴리곤은 잘리지 않는다!)

            else//조금이라도 절단 면에 걸린다면 이 과정을 거친다. 폴리곤을 자르는 과정이다.
            {
                //같이 있는 두 정점과, 혼자 있는 한 정점의 정보를 준비한다.
                //Mathf.Sign => 양수이거나 0이면 1을 반환하고, 음수이면 -1을 반환합니다.
                //삼항 연산자 => ?기준 앞의 전제에 따라 true : false 
                int aloneVertIdx = Mathf.Sign(dot0) == Mathf.Sign(dot1) ? vertIdx2 : (Mathf.Sign(dot0) == Mathf.Sign(dot2) ? vertIdx1 : vertIdx0);
                int otherVertIdx0 = Mathf.Sign(dot0) == Mathf.Sign(dot1) ? vertIdx0 : (Mathf.Sign(dot0) == Mathf.Sign(dot2) ? vertIdx2 : vertIdx1);
                int otherVertIdx1 = Mathf.Sign(dot0) == Mathf.Sign(dot1) ? vertIdx1 : (Mathf.Sign(dot0) == Mathf.Sign(dot2) ? vertIdx0 : vertIdx2);
                //폴리곤을 어떤 식으로 자르든 한개의 정점만이 따로 있다.
                Vector3 aloneVert = orinVerts[aloneVertIdx];
                Vector3 otherVert0 = orinVerts[otherVertIdx0];
                Vector3 otherVert1 = orinVerts[otherVertIdx1];
                Vector3 aloneNor = orinNors[aloneVertIdx];
                Vector3 otherNor0 = orinNors[otherVertIdx0];
                Vector3 otherNor1 = orinNors[otherVertIdx1];
                Vector2 aloneUv = orinUvs[aloneVertIdx];
                Vector2 otherUv0 = orinUvs[otherVertIdx0];
                Vector2 otherUv1 = orinUvs[otherVertIdx1];

                float alone2PlaneDist = Mathf.Abs(Vector3.Dot(_sliceNormal, aloneVert - _slicePoint));
                float other02PlaneDist = Mathf.Abs(Vector3.Dot(_sliceNormal, otherVert0 - _slicePoint));
                float other12PlaneDist = Mathf.Abs(Vector3.Dot(_sliceNormal, otherVert1 - _slicePoint));
                float alone2Other0Ratio = alone2PlaneDist / (alone2PlaneDist + other02PlaneDist);
                float alone2Other1Ratio = alone2PlaneDist / (alone2PlaneDist + other12PlaneDist);
                //혼자있는 정점과 다른 한점의 내적 비율을 이용하여, 선형보간을 통해 단면위에 올라가 있는 점을 구한다.

                //Vector3.Learp(선형보간) t = 0일때 t을 반환합니다. t = 1 일때 to 를 반환합니다. t = 0.5 일때 from 과 to 사이의 중간 지점을 반환합니다.
                Vector3 createdVert0 = Vector3.Lerp(aloneVert, otherVert0, alone2Other0Ratio);
                Vector3 createdVert1 = Vector3.Lerp(aloneVert, otherVert1, alone2Other1Ratio);
                Vector3 createdNor0 = Vector3.Lerp(aloneNor, otherNor0, alone2Other0Ratio);
                Vector3 createdNor1 = Vector3.Lerp(aloneNor, otherNor1, alone2Other1Ratio);
                Vector2 createdUv0 = Vector2.Lerp(aloneUv, otherUv0, alone2Other0Ratio);
                Vector2 createdUv1 = Vector2.Lerp(aloneUv, otherUv1, alone2Other1Ratio);
                createdVerts.Add(createdVert0);
                createdVerts.Add(createdVert1);
                createdNors.Add(createdNor0);
                createdNors.Add(createdNor1);
                createdUvs.Add(createdUv0);
                createdUvs.Add(createdUv1);

                //혼자 다른 방향에 있는 정점의 위치에 따라 다르게 넣어준다.
                float aloneSide = Vector3.Dot(_sliceNormal, aloneVert - _slicePoint);
                if (aloneSide < 0)
                {
                    //A side
                    aSideVerts.Add(aloneVert);
                    aSideVerts.Add(createdVert0);
                    aSideVerts.Add(createdVert1);
                    aSideNors.Add(aloneNor);
                    aSideNors.Add(createdNor0);
                    aSideNors.Add(createdNor1);
                    aSideUvs.Add(aloneUv);
                    aSideUvs.Add(createdUv0);
                    aSideUvs.Add(createdUv1);
                    aSideTris.Add(aSideTris.Count);
                    aSideTris.Add(aSideTris.Count);
                    aSideTris.Add(aSideTris.Count);

                    //B side
                    bSideVerts.Add(otherVert0);
                    bSideVerts.Add(otherVert1);
                    bSideVerts.Add(createdVert0);
                    bSideNors.Add(otherNor0);
                    bSideNors.Add(otherNor1);
                    bSideNors.Add(createdNor0);
                    bSideUvs.Add(otherUv0);
                    bSideUvs.Add(otherUv1);
                    bSideUvs.Add(createdUv0);
                    bSideTris.Add(bSideTris.Count);
                    bSideTris.Add(bSideTris.Count);
                    bSideTris.Add(bSideTris.Count);

                    bSideVerts.Add(otherVert1);
                    bSideVerts.Add(createdVert1);
                    bSideVerts.Add(createdVert0);
                    bSideNors.Add(otherNor1);
                    bSideNors.Add(createdNor1);
                    bSideNors.Add(createdNor0);
                    bSideUvs.Add(otherUv1);
                    bSideUvs.Add(createdUv1);
                    bSideUvs.Add(createdUv0);
                    bSideTris.Add(bSideTris.Count);
                    bSideTris.Add(bSideTris.Count);
                    bSideTris.Add(bSideTris.Count);
                }
                else
                {
                    //B side
                    bSideVerts.Add(aloneVert);
                    bSideVerts.Add(createdVert0);
                    bSideVerts.Add(createdVert1);
                    bSideNors.Add(aloneNor);
                    bSideNors.Add(createdNor0);
                    bSideNors.Add(createdNor1);
                    bSideUvs.Add(aloneUv);
                    bSideUvs.Add(createdUv0);
                    bSideUvs.Add(createdUv1);
                    bSideTris.Add(bSideTris.Count);
                    bSideTris.Add(bSideTris.Count);
                    bSideTris.Add(bSideTris.Count);

                    //A side
                    aSideVerts.Add(otherVert0);
                    aSideVerts.Add(otherVert1);
                    aSideVerts.Add(createdVert0);
                    aSideNors.Add(otherNor0);
                    aSideNors.Add(otherNor1);
                    aSideNors.Add(createdNor0);
                    aSideUvs.Add(otherUv0);
                    aSideUvs.Add(otherUv1);
                    aSideUvs.Add(createdUv0);
                    aSideTris.Add(aSideTris.Count);
                    aSideTris.Add(aSideTris.Count);
                    aSideTris.Add(aSideTris.Count);

                    aSideVerts.Add(otherVert1);
                    aSideVerts.Add(createdVert1);
                    aSideVerts.Add(createdVert0);
                    aSideNors.Add(otherNor1);
                    aSideNors.Add(createdNor1);
                    aSideNors.Add(createdNor0);
                    aSideUvs.Add(otherUv1);
                    aSideUvs.Add(createdUv1);
                    aSideUvs.Add(createdUv0);
                    aSideTris.Add(aSideTris.Count);
                    aSideTris.Add(aSideTris.Count);
                    aSideTris.Add(aSideTris.Count);
                }
            }
        }//for문 끝

        List<Vector3> sortedCreatedVerts;
        SortVertices(createdVerts, out sortedCreatedVerts);

        List<Vector3> aSideCapVerts, bSideCapVerts;
        List<Vector3> aSideCapNors, bSideCapNors;
        List<Vector2> aSideCapUvs, bSideCapUvs;
        List<int> aSideCapTris, bSideCapTris;

        MakeCap(_sliceNormal, sortedCreatedVerts, out aSideCapVerts, out bSideCapVerts, out aSideCapNors, out bSideCapNors, out aSideCapUvs, out bSideCapUvs, out aSideCapTris, out bSideCapTris);
        //기존의 List에 Cap의 값들을 붙혀준다.
        aSideVerts.AddRange(aSideCapVerts);
        aSideNors.AddRange(aSideCapNors);
        aSideUvs.AddRange(aSideCapUvs);
        for (int i = 0; i < aSideCapTris.Count; i++)
        {
            aSideCapTris[i] = aSideCapTris[i] + aSideTris.Count;//CapTris의 값이 시작값으로 되어있기 때문에,뒤에 기존 Tris의 수 만큼 뒤로 미뤄준다.
        }

        bSideVerts.AddRange(bSideCapVerts);
        bSideNors.AddRange(bSideCapNors);
        bSideUvs.AddRange(bSideCapUvs);
        for (int i = 0; i < bSideCapTris.Count; i++)
        {
            bSideCapTris[i] = bSideCapTris[i] + bSideTris.Count;
        }

        //세팅된 값들을 통해 Aside와 Bside를 그리는 과정
        Mesh aMesh = new Mesh();
        Mesh bMesh = new Mesh();
        aMesh.vertices = aSideVerts.ToArray();
        aMesh.normals = aSideNors.ToArray();
        aMesh.uv = aSideUvs.ToArray();
        aMesh.subMeshCount = _target.GetComponent<MeshRenderer>().sharedMaterials.Length + 1; //Sub mesh의 개수를 하나 늘린다.
        aMesh.SetTriangles(aSideTris, 0);
        aMesh.SetTriangles(aSideCapTris, _target.GetComponent<MeshRenderer>().sharedMaterials.Length); //Sub mesh에 단면을 넣는다.
        bMesh.vertices = bSideVerts.ToArray();
        bMesh.normals = bSideNors.ToArray();
        bMesh.uv = bSideUvs.ToArray();
        bMesh.subMeshCount = _target.GetComponent<MeshRenderer>().sharedMaterials.Length + 1; //Sub mesh의 개수를 하나 늘린다.
        bMesh.SetTriangles(bSideTris, 0);
        bMesh.SetTriangles(bSideCapTris, _target.GetComponent<MeshRenderer>().sharedMaterials.Length); //Sub mesh에 단면을 넣는다.

        GameObject aObject = new GameObject(_target.name + "_A", typeof(MeshFilter), typeof(MeshRenderer));
        GameObject bObject = new GameObject(_target.name + "_B", typeof(MeshFilter), typeof(MeshRenderer));
        Material[] mats = new Material[_target.GetComponent<MeshRenderer>().sharedMaterials.Length + 1];
        for (int i = 0; i < _target.GetComponent<MeshRenderer>().sharedMaterials.Length; i++)
        {
            mats[i] = _target.GetComponent<MeshRenderer>().sharedMaterials[i];
        }
        mats[_target.GetComponent<MeshRenderer>().sharedMaterials.Length] = _ineterial; //마지막에 단면에 쓰일 material을 적용
        aObject.GetComponent<MeshFilter>().sharedMesh = aMesh;
        aObject.GetComponent<MeshRenderer>().sharedMaterials = mats;
        bObject.GetComponent<MeshFilter>().sharedMesh = bMesh;
        bObject.GetComponent<MeshRenderer>().sharedMaterials = mats;
        aObject.transform.position = _target.transform.position;
        aObject.transform.rotation = _target.transform.rotation;
        aObject.transform.localScale = _target.transform.localScale;
        bObject.transform.position = _target.transform.position;
        bObject.transform.rotation = _target.transform.rotation;
        bObject.transform.localScale = _target.transform.localScale;

        _target.SetActive(false);//기존에 있던 Cube를 비활성화

        return new GameObject[] { aObject, bObject };//잘린 오브젝트들 반환

    }
    internal static void SortVertices(List<Vector3> _target, out List<Vector3> _result)//새로 생긴 정점들이 겹치는 경우가 있기 때문에 이를 제거하고 순서를 정한다.
                                                                                       //두개의 선을 비교하여 끝자락의 어떤 점이라도 겹친다면 그 선분을 잇는 형식으로 이루어진다. 이 과정에서 겹치는 점을 제거된다.
    {
        _result = new List<Vector3>();
        _result.Add(_target[0]);
        _result.Add(_target[1]);
        int vertSetCount = _target.Count / 2;
        for (int i = 0; i < vertSetCount - 1; i++)
        {
            Vector3 vert0 = _target[i * 2];
            Vector3 vert1 = _target[i * 2 + 1];
            for (int j = i + 1; j < vertSetCount; j++)
            {
                Vector3 cVert0 = _target[j * 2];
                Vector3 cVert1 = _target[j * 2 + 1];
                if (vert1 == cVert0)
                {
                    _result.Add(cVert1);

                    SwapTwoIndexSet<Vector3>(ref _target, i * 2 + 2, i * 2 + 3, j * 2, j * 2 + 1);
                }
                else if (vert1 == cVert1)
                {
                    _result.Add(cVert0);

                    SwapTwoIndexSet<Vector3>(ref _target, i * 2 + 2, i * 2 + 3, j * 2 + 1, j * 2);
                }
            }
        }
        if (_result[0] == _result[_result.Count - 1]) _result.RemoveAt(_result.Count - 1);
    }

    internal static void SwapTwoIndexSet<T>(ref List<T> _target, int _idx00, int _idx01, int _idx10, int _idx11)
    {
        T temp0 = _target[_idx00];
        T temp1 = _target[_idx01];
        _target[_idx00] = _target[_idx10];
        _target[_idx01] = _target[_idx11];
        _target[_idx10] = temp0;
        _target[_idx11] = temp1;
    }
    internal static void MakeCap(Vector3 _faceNormal, List<Vector3> _relatedVerts,
        out List<Vector3> _aSideVerts, out List<Vector3> _bSideVerts,
        out List<Vector3> _aSideNors, out List<Vector3> _bSideNors,
        out List<Vector2> _aSideUvs, out List<Vector2> _bSideUvs,
        out List<int> _aSideTris, out List<int> _bSideTris)//중앙에 정점을 구한뒤 그 정점과 나머지 정정들을 이어서 폴리곤을 형성한다.
    {
        _aSideVerts = new List<Vector3>();
        _bSideVerts = new List<Vector3>();
        _aSideNors = new List<Vector3>();
        _bSideNors = new List<Vector3>();
        _aSideUvs = new List<Vector2>();
        _bSideUvs = new List<Vector2>();
        _aSideTris = new List<int>();
        _bSideTris = new List<int>();
        _aSideVerts.AddRange(_relatedVerts);
        _bSideVerts.AddRange(_relatedVerts);
        if (_relatedVerts.Count < 2) return;

        //Calculate center of the cap
        Vector3 center = Vector3.zero;
        foreach (Vector3 v in _relatedVerts)
        {
            center += v;
        }
        center /= _relatedVerts.Count;
        //Add center vert to both side at last
        _aSideVerts.Add(center);
        _bSideVerts.Add(center);

        //Calculate cap data
        //Normal
        for (int i = 0; i < _aSideVerts.Count; i++)
        {
            _aSideNors.Add(_faceNormal);
            _bSideNors.Add(-_faceNormal);
        }
        //Uv
        //Basis on sliced plane
        Vector3 forward = Vector3.zero;
        forward.x = _faceNormal.y;
        forward.y = -_faceNormal.x;
        forward.z = _faceNormal.z;
        Vector3 left = Vector3.Cross(forward, _faceNormal);
        for (int i = 0; i < _relatedVerts.Count; i++)
        {
            Vector3 dir = _relatedVerts[i] - center;
            Vector2 relatedUV = Vector2.zero;
            relatedUV.x = 0.5f + Vector3.Dot(dir, left);
            relatedUV.y = 0.5f + Vector3.Dot(dir, forward);
            _aSideUvs.Add(relatedUV);
            _bSideUvs.Add(relatedUV);
        }
        _aSideUvs.Add(new Vector2(0.5f, 0.5f));
        _bSideUvs.Add(new Vector2(0.5f, 0.5f));
        //Triangle
        int centerIdx = _aSideVerts.Count - 1;
        //Check first triangle face where
        float faceDir = Vector3.Dot(_faceNormal, Vector3.Cross(_relatedVerts[0] - center, _relatedVerts[1] - _relatedVerts[0]));
        //Store tris
        for (int i = 0; i < _aSideVerts.Count - 1; i++)
        {
            int idx0 = i;
            int idx1 = (i + 1) % (_aSideVerts.Count - 1);
            if (faceDir < 0)
            {
                _aSideTris.Add(centerIdx);
                _aSideTris.Add(idx1);
                _aSideTris.Add(idx0);

                _bSideTris.Add(centerIdx);
                _bSideTris.Add(idx0);
                _bSideTris.Add(idx1);
            }
            else
            {
                _aSideTris.Add(centerIdx);
                _aSideTris.Add(idx0);
                _aSideTris.Add(idx1);

                _bSideTris.Add(centerIdx);
                _bSideTris.Add(idx1);
                _bSideTris.Add(idx0);
            }
        }
    }
}