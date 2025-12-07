using System.Linq;
using UnityEngine;
/**
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

public class FaceMatcher
{
    private InferenceSession session;

    public FaceMatcher(string onnxPath)
    {
        session = new InferenceSession(onnxPath);
    }

    // 1. 对齐关键点（示例：眼睛对齐）
    private Texture2D AlignFace(Texture2D faceImage, Vector3[] landmarks)
    {
        // 简化示例：你可以用眼睛和嘴角做仿射变换
        // 这里假设已经有一个对齐后的 faceImage
        return faceImage;
    }

    // 2. 将 Texture2D 转为 float[]（112x112 RGB）
    private float[] ImageToFloatArray(Texture2D img)
    {
        Color32[] pixels = img.GetPixels32();
        float[] data = new float[112 * 112 * 3];
        for (int i = 0; i < pixels.Length; i++)
        {
            data[i * 3 + 0] = (pixels[i].r - 127.5f) / 128f;
            data[i * 3 + 1] = (pixels[i].g - 127.5f) / 128f;
            data[i * 3 + 2] = (pixels[i].b - 127.5f) / 128f;
        }
        return data;
    }

    // 3. 获取 ArcFace 特征向量
    private float[] GetFeatureVector(Texture2D alignedFace)
    {
        float[] inputData = ImageToFloatArray(alignedFace);
        var tensor = new DenseTensor<float>(inputData, new int[] { 1, 3, 112, 112 });
        var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input", tensor) };
        using var results = session.Run(inputs);
        return results.First().AsEnumerable<float>().ToArray();
    }

    // 4. 计算余弦相似度
    private float CosineSimilarity(float[] vec1, float[] vec2)
    {
        float dot = 0f, norm1 = 0f, norm2 = 0f;
        for (int i = 0; i < vec1.Length; i++)
        {
            dot += vec1[i] * vec2[i];
            norm1 += vec1[i] * vec1[i];
            norm2 += vec2[i] * vec2[i];
        }
        return dot / (Mathf.Sqrt(norm1) * Mathf.Sqrt(norm2));
    }

    // 5. 外部调用方法：输入两个关键点数组，输出匹配置信度
    public float MatchFaces(Texture2D face1, Vector3[] landmarks1, Texture2D face2, Vector3[] landmarks2)
    {
        Texture2D aligned1 = AlignFace(face1, landmarks1);
        Texture2D aligned2 = AlignFace(face2, landmarks2);
        float[] feat1 = GetFeatureVector(aligned1);
        float[] feat2 = GetFeatureVector(aligned2);
        return CosineSimilarity(feat1, feat2); // 0~1 越大越相似
    }
}
*/