using UnityEngine;

namespace RavenCameraFX
{
    public class SecondOrder
    {
        private Vector3 xp;
        public Vector3 y, yd;
        public float k1, k2, k3;

        private float PI = 3.14159f;

        private float T_crit;

        public SecondOrder(float f, float z, float r, Vector3 x0)
        {
            k1 = z / (PI * f);
            k2 = 1 / ((2 * PI * f) * (2 * PI * f));
            k3 = r * z / (2 * PI * f);

            T_crit = 0.8f * (Mathf.Sqrt(4 * k2 + k1 * k1) - k1);


            xp = x0;
            y = x0;
            yd = Vector3.zero;
        }

        public Vector3 Update(float T, Vector3 x)
        {
            Vector3 xd;
            xd = (x - xp) / T;
            xp = x;
            int iterations = Mathf.CeilToInt(T / T_crit);
            T = T / iterations;
            for (int i = 0; i < iterations; i++)
            {
                y = y + T * yd;
                yd = yd + T * (x + k3 * xd - y - k1 * yd) / k2;
            }
            return y;
        }
    }
}
