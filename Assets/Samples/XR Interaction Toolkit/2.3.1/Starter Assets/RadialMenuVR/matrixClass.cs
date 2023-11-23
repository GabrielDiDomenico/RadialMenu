using System.Collections.Generic;
using System;
using UnityEngine;

public class matrixClass : MonoBehaviour
{
    [SerializeField] private List<List<float>> m;

    public matrixClass(int n)
    {
        FillMatrix(n);
    }

    public void FillMatrix(int mSize)
    {
        m = new List<List<float>>();
        List<float> aux = new List<float>();

        for (int i = 0; i < mSize; i++)
        {
            aux.Add(0);
        }

        for (int i = 0; i < mSize; i++)
        {
            m.Add(new List<float>(aux));
            m[i][i] = 1;
        }
    }

    public void PrintMatrix()
    {
        for (int i = 0; i < m.Count; i++)
        {
            for (int j = 0; j < m[i].Count; j++)
            {
                Console.Write(m[i][j] + " = ");
            }
            Console.WriteLine();
        }
    }

    public void MatrixTranslate(float tX, float tY)
    {
        List<List<float>> mTranslate = new List<List<float>>()
        {
            new List<float>(){1, 0, tX},
            new List<float>(){0, 1, tY},
            new List<float>(){0, 0, 1}
        };

        List<List<float>> mReturn = new List<List<float>>();
        List<float> auxC = new List<float>();
        float sum = 0;

        for (int i = 0; i < mTranslate.Count; i++)
        {
            for (int j = 0; j < m.Count; j++)
            {
                for (int k = 0; k < m[j].Count; k++)
                {
                    sum += mTranslate[i][k] * m[k][j];
                }
                auxC.Add(sum);
                sum = 0;
            }
            mReturn.Add(new List<float>(auxC));
            auxC.Clear();
        }

        m = mReturn;
    }

    public void MatrixRotate(double ang)
    {
        List<List<float>> mRotate = new List<List<float>>()
        {
            new List<float>(){(float)Math.Cos(ang), -(float)Math.Sin(ang), 0},
            new List<float>(){(float)Math.Sin(ang), (float)Math.Cos(ang), 0},
            new List<float>(){0, 0, 1}
        };

        List<List<float>> mReturn = new List<List<float>>();
        List<float> auxC = new List<float>();
        float sum = 0;

        for (int i = 0; i < mRotate.Count; i++)
        {
            for (int j = 0; j < m.Count; j++)
            {
                for (int k = 0; k < m[j].Count; k++)
                {
                    sum += mRotate[i][k] * m[k][j];
                }
                auxC.Add(sum);
                sum = 0;
            }
            mReturn.Add(new List<float>(auxC));
            auxC.Clear();
        }

        m = mReturn;
    }

    public List<List<float>> CalcPonto(List<List<float>> p)
    {
        List<List<float>> mReturn = new List<List<float>>();
        List<float> auxC = new List<float>();
        float sum = 0;

        for (int i = 0; i < m.Count; i++)
        {
            for (int j = 0; j < m[i].Count; j++)
            {
                sum += m[i][j] * p[j][0];
            }
            auxC.Add(sum);
            sum = 0;
        }
        mReturn.Add(auxC);

        return mReturn;
    }

    public void SetMatrixToBezier4()
    {
        m.Clear();
        m.Add(new List<float> { 1, 0, 0, 0 });
        m.Add(new List<float> { -3, 3, 0, 0 });
        m.Add(new List<float> { 3, -6, 3, 0 });
        m.Add(new List<float> { -1, 3, -3, 1 });
    }

    public void InitMatrixBezier4(List<List<float>> p)
    {
        List<List<float>> mReturn = new List<List<float>>();
        List<float> auxC = new List<float>();
        float sum = 0;

        SetMatrixToBezier4();
        
        for (int i = 0; i < m.Count; i++)
        {
            for (int j = 0; j < p[0].Count; j++)
            {
                for (int k = 0; k < m[0].Count; k++)
                {
                    sum += m[i][k] * p[k][j];
                }
                
                auxC.Add(sum);
                sum = 0;
            }
            
            mReturn.Add(auxC);
            Debug.Log(mReturn[i].Count);
            auxC.Clear();
        }
        m = mReturn;
        Debug.Log("m tam: " + m.Count + "mL tam: " + m[0].Count);
    }

    public List<List<float>> CalcED(List<List<float>> p)
    {
        List<List<float>> mReturn = new List<List<float>>();
        List<float> auxC = new List<float>();
        float sum = 0;

        for (int i = 0; i < p.Count; i++)
        {
            for (int j = 0; j < m[0].Count; j++)
            {
                for (int k = 0; k < p[0].Count; k++)
                {
                    sum += p[i][k] * m[k][j];
                }
                auxC.Add(sum);
                
                sum = 0;
            }
            
            mReturn.Add(auxC);
            auxC.Clear();
        }
        
        return mReturn;
    }

    public void Reset()
    {
        m.Clear();
    }
}

