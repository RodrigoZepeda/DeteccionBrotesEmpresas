using System;
using System.Linq;
using MathNet.Numerics; //https://numerics.mathdotnet.com

//Desarrollado por Rodrigo Zepeda:
//rodrigo.zepeda@imss.gob.mx
//Adaptación del algoritmo EARS-C1 del CDC
//Referencia:
//Introduction to Statistical Methods for Biosurveillance: With an Emphasis on Syndromic Surveillance


namespace DeteccionBrotesEmpresas
{
    public class CasosEmpresa
    {
        //Casos registrados en la empresa
        public int[] Casos { get; set; }

        //Valor alpha relacionado a la confianza del metodo (1 - alpha)*100%
        private double Alpha { get; set; }

        //Valor de ruido mínimo
        private double Epsilon { get; set; }

        //Valor de días previos
        private int Dias_previos { get; set; }

        //Constructor con sólo casos
        public CasosEmpresa(int[] casos)
        {
            this.Casos = casos;
            this.Alpha = 0.05;
            this.Epsilon = 0.0;
            this.Dias_previos = 7;
        }

        //Constructor con todas las variables
        public CasosEmpresa(int[] casos, double alpha, double epsilon, int dias_previos)
        {
            this.Casos = casos;
            this.Alpha = alpha;
            this.Epsilon = epsilon;
            this.Dias_previos = dias_previos;
        }

        //Función para generar una alerta
        public bool[] Alerta()
        {
            return EARSC1(Casos, Alpha, Epsilon, Dias_previos);
        }

        //Función EARS-C1 para el cálculo de alertas
        private bool[] EARSC1(int[] casos, double alpha, double epsilon, int dias_previos)
        {
            bool[] alert = new bool[casos.Length];
            double[] doublecasos = Array.ConvertAll<int, double>(casos, x => x);

            if (dias_previos < casos.Length)
            {
                //Cálculo de media móvil, desv est móvil y casos
                double[] Ybar = MediaMovil(doublecasos, dias_previos);
                double[] sd   = DesviacionEstandarMovil(doublecasos, dias_previos, epsilon);
                double[] Y = new ArraySegment<double>(doublecasos, dias_previos - 1, doublecasos.Length - dias_previos + 1).ToArray();
                double Z = Zval(1 - alpha);

                for (int i = 1; i < Y.Length; i++)
                {
                    alert[i - 1 + dias_previos] = Y[i] > (Ybar[i - 1] + Z * sd[i - 1]);
                }

            }
            else
            {
                throw new System.InvalidOperationException($"Cantidad de casos {casos.Length} es menor a dias_previos = {dias_previos}");
            }
            return alert;
        }

        private double[] MediaMovil(double[] x, int t)
        {
            double[] mediamovil = new double[0];
            if (x.Length >= t)
            {
                //Cálculo de la longitud del vector resultante de media móvil
                int diff = x.Length - (t - 1);

                //Vector resultante de medias
                mediamovil = new double[diff];

                for (int i = 0; i < diff; i++)
                {
                    double[] subX = new ArraySegment<double>(x, i, t).ToArray();
                    mediamovil[i] = Media(subX);
                }

            }

            return mediamovil;
        }

        //Función que devuelve una normal evaluada en el cuantil
        //https://stackoverflow.com/questions/1662943/standard-normal-distribution-z-value-function-in-c-sharp
        private double Zval(double quantile)
        {
            var curve = new MathNet.Numerics.Distributions.Normal();
            var z_value = curve.InverseCumulativeDistribution(quantile);
            return z_value;
        }

        private double[] DesviacionEstandarMovil(double[] x, int t, double epsilon)
        {
            double[] desvestmovil = new double[0];
            if (x.Length >= t)
            {
                //Cálculo de la longitud del vector resultante de media móvil
                int diff = x.Length - (t - 1);

                //Vector resultante de medias
                desvestmovil = new double[diff];

                for (int i = 0; i < diff; i++)
                {
                    double[] subX = new ArraySegment<double>(x, i, t).ToArray();
                    desvestmovil[i] = DesviacionEstandar(subX, epsilon);
                }

            }

            return desvestmovil;
        }

        private double[] DesviacionEstandarMovil(double[] x, int t)
        {
            return DesviacionEstandarMovil(x, t, 0.0);
        }

        //Función para estimar el promedio a partir de un array de tamaño N
        private double Media(double[] x)
        {
            double mu = 0;
            int N = x.Length;
            for (int i = 0; i < N; i++)
            {
                mu = mu + x[i] / N;
            }
            return mu;
        }

        //Función para estimar la desviación estándar a partir de un array de tamaño N
        //devuelve el máximo entre desvest y un parámetro epsilon
        private double DesviacionEstandar(double[] x, double epsilon)
        {
            double desvest = 0;
            int N = x.Length;
            if (N > 1)
            {
                double media = Media(x);
                for (int i = 0; i < N; i++)
                {
                    desvest = desvest + Math.Pow((x[i] - media), 2) / (N - 1);
                }
                desvest = Math.Sqrt(desvest);
            }
            return Math.Max(desvest, epsilon);
        }

        //Para cuando epsilon es cero
        private double DesviacionEstandar(double[] x)
        {
            return DesviacionEstandar(x, 0.0);
        }

        //Test para la función de medias
        private bool TestMedia()
        {
            Console.WriteLine("Probando Media");

            //Test 1
            double tol = 0.00001;
            double[] x = { 0, 0, 0, 0, 0, 0, 0, 0 };
            double mu = Media(x);
            if (!(Math.Abs(mu - 0.0) < tol))
            {
                throw new System.InvalidOperationException("Error en test 1");
            }

            //Test 2
            tol = 0.00001;
            x = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0 };
            mu = Media(x);
            if (!(Math.Abs(mu - 5.5) < tol))
            {
                throw new System.InvalidOperationException("Error en test 2");
            }

            //Test 3
            tol = 0.00001;
            x = new double[] { -2, -1, 1, 2, 3 };
            mu = Media(x);
            if (!(Math.Abs(mu - 0.6) < tol))
            {
                throw new System.InvalidOperationException("Error en test 3");
            }

            return true;
        }

        //Test para la función de medias
        private bool TestZval()
        {
            Console.WriteLine("Probando Zval");

            //Test 1 por valores
            double tol = 0.00001;
            double Z = Zval(0.975);
            if (!(Math.Abs(Z - 1.959964) < tol))
            {
                throw new System.InvalidOperationException("Error en test 1");
            }

            //Test 2 por valores
            tol = 0.00001;
            Z = Zval(0.5);
            if (!(Math.Abs(Z - 0.0) < tol))
            {
                throw new System.InvalidOperationException("Error en test 2");
            }

            return true;

        }

        //Test para la función de media móvil
        private bool TestMediaMovil()
        {
            Console.WriteLine("Probando MediaMovil");

            //Test 1 (longitud)
            double[] x = { 1, 2, 3, 4, 5 };
            double[] mu = MediaMovil(x, 2);
            if (mu.Length != 4)
            {
                throw new System.InvalidOperationException("Error en test 1");
            }

            //Test 2 (valores)
            double tol = 0.00001;
            double[] trueval = { 1.5, 2.5, 3.5, 4.5 };
            x = new double[] { 1, 2, 3, 4, 5 };
            mu = MediaMovil(x, 2);
            for (int i = 0; i < mu.Length; i++)
            {
                if (!(Math.Abs(mu[i] - trueval[i]) < tol))
                {
                    throw new System.InvalidOperationException("Error en test 2");
                }
            }

            //Test 3 (valores)
            tol = 0.00001;
            trueval = new double[] { 2, 3, 4 };
            x = new double[] { 1, 2, 3, 4, 5 };
            mu = MediaMovil(x, 3);
            for (int i = 0; i < mu.Length; i++)
            {
                if (!(Math.Abs(mu[i] - trueval[i]) < tol))
                {
                    throw new System.InvalidOperationException("Error en test 3");
                }
            }

            //Test 4 (inicial)
            tol = 0.00001;
            x = new double[] { 1, 2, 3, 4, 5 };
            mu = MediaMovil(x, 1);
            for (int i = 0; i < mu.Length; i++)
            {
                if (!(Math.Abs(mu[i] - x[i]) < tol))
                {
                    throw new System.InvalidOperationException("Error en test 4");
                }
            }

            //Test 5 (final)
            tol = 0.00001;
            x = new double[] { 1, 2, 3, 4, 5 };
            mu = MediaMovil(x, x.Length);
            if (!(Math.Abs(mu[0] - 3) < tol))
            {
                throw new System.InvalidOperationException("Error en test 5");
            }

            //Test 6 (sinsentido)
            tol = 0.00001;
            x = new double[] { 1, 2, 3, 4, 5 };
            mu = MediaMovil(x, x.Length + 1);
            if (mu.Length > 0)
            {
                throw new System.InvalidOperationException("Error en test 6");
            }


            return true;

        }

        //Test para la función de desviación estándar
        private bool TestDesviacionEstandar()
        {
            Console.WriteLine("Probando DesviacionEstandar");

            //Test 1
            double tol = 0.00001;
            double[] x = { 0, 0, 0, 0, 0, 0, 0, 0 };
            double sd = DesviacionEstandar(x);
            if (!(Math.Abs(sd - 0.0) < tol))
            {
                throw new System.InvalidOperationException("Error en test 1");
            }

            //Test 2
            tol = 0.00001;
            x = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0 };
            sd = DesviacionEstandar(x);
            if (!(Math.Abs(sd - 3.0276503540974917) < tol))
            {
                throw new System.InvalidOperationException("Error en test 2");
            }

            //Test 3
            tol = 0.00001;
            x = new double[] { -2, -1, 1, 2, 3 };
            sd = DesviacionEstandar(x);
            if (!(Math.Abs(sd - 2.073644) < tol))
            {
                throw new System.InvalidOperationException("Error en test 3");
            }

            //Test 4
            tol = 0.00001;
            x = new double[] { 1 };
            sd = DesviacionEstandar(x);
            if (!(Math.Abs(sd - 0.0) < tol))
            {
                throw new System.InvalidOperationException("Error en test 4");
            }

            return true;
        }

        //Test para la función de media móvil
        private bool TestDesviacionEstandarMovil()
        {
            Console.WriteLine("Probando DesviacionEstandarMovil");
            bool bandera = true;

            //Test 1 (longitud)
            double[] x = { 1, 2, 3, 4, 5 };
            double[] sd = DesviacionEstandarMovil(x, 2);
            if (sd.Length != 4)
            {
                throw new System.InvalidOperationException("Error en test 1");
            }

            //Test 2 (valores)
            double tol = 0.00001;
            double[] trueval = { 0.7071067811865476, 0.7071067811865476, 0.7071067811865476, 0.7071067811865476 };
            x = new double[] { 1, 2, 3, 4, 5 };
            sd = DesviacionEstandarMovil(x, 2);
            for (int i = 0; i < sd.Length; i++)
            {
                if (!(Math.Abs(sd[i] - trueval[i]) < tol))
                {
                    throw new System.InvalidOperationException("Error en test 2");
                }
            }

            //Test 3 (valores)
            tol = 0.00001;
            trueval = new double[] { 1.0, 1.0, 1.0 };
            x = new double[] { 1, 2, 3, 4, 5 };
            sd = DesviacionEstandarMovil(x, 3);
            for (int i = 0; i < sd.Length; i++)
            {
                if (!(Math.Abs(sd[i] - trueval[i]) < tol))
                {
                    throw new System.InvalidOperationException("Error en test 3");
                }
            }

            //Test 4 (inicial)
            tol = 0.00001;
            x = new double[] { 1, 2, 3, 4, 5 };
            sd = DesviacionEstandarMovil(x, 1);
            for (int i = 0; i < sd.Length; i++)
            {
                if (!(Math.Abs(sd[i] - 0.0) < tol))
                {
                    throw new System.InvalidOperationException("Error en test 4");
                }
            }

            //Test 5 (final)
            tol = 0.00001;
            x = new double[] { 1, 2, 3, 4, 5 };
            sd = DesviacionEstandarMovil(x, x.Length);
            if (!(Math.Abs(sd[0] - 1.5811388300841898) < tol))
            {
                throw new System.InvalidOperationException("Error en test 5");
            }

            //Test 6 (sinsentido)
            tol = 0.00001;
            x = new double[] { 1, 2, 3, 4, 5 };
            sd = DesviacionEstandarMovil(x, x.Length + 1);
            if (sd.Length > 0)
            {
                throw new System.InvalidOperationException("Error en test 6");
            }

            return true;

        }

        private bool TestEARSC1()
        {

            Console.WriteLine("Probando EARSC1");

            //Test 1 (longitud)
            int[] x = { 1, 2, 3, 4, 5, 6, 7, 8 };
            bool[] ears = EARSC1(x, 0.05, 0.0, 7);
            if (ears.Length != x.Length)
            {
                throw new System.InvalidOperationException("Error en test 1");
            }

            //Test 2 (valores)
            double tol = 0.00001;
            bool[] trueval = { false, false, false, false, false, false, false, false, false, false };
            x = new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            ears = EARSC1(x, 0.05, 0.0, 7);
            for (int i = 0; i < ears.Length; i++)
            {
                if (!(Math.Abs(ears[i].CompareTo(trueval[i]) - 0.0) < tol))
                {
                    throw new System.InvalidOperationException("Error en test 2");
                }
            }

            //Test 3 (valores)
            tol = 0.00001;
            trueval = new bool[] { false, false, false, false, false, false, false, true };
            x = new int[] { 0, 0, 0, 0, 0, 0, 0, 1 };
            ears = EARSC1(x, 0.05, 0.0, 7);
            for (int i = 0; i < ears.Length; i++)
            {
                if (!(Math.Abs(ears[i].CompareTo(trueval[i]) - 0.0) < tol))
                {
                    throw new System.InvalidOperationException("Error en test 3");
                }
            }

            //Test 4 (valores)
            tol = 0.00001;
            trueval = new bool[] { false, false, false, false, false, false, false, true, false, false, false };
            x = new int[] { 0, 0, 0, 0, 0, 0, 0, 10, 0, 0, 1 };
            ears = EARSC1(x, 0.05, 0.0, 7);
            for (int i = 0; i < ears.Length; i++)
            {
                if (!(Math.Abs(ears[i].CompareTo(trueval[i]) - 0.0) < tol))
                {
                    throw new System.InvalidOperationException("Error en test 4");
                }
            }

            //Test 5 (valores)
            tol = 0.00001;
            trueval = new bool[] { false, false, false, false, false, false, false, true, false, false, false };
            x = new int[] { 0, 0, 0, 0, 0, 0, 0, 10, 0, 0, 1 };
            ears = EARSC1(x, 0.05, 0.0, 5);
            for (int i = 0; i < ears.Length; i++)
            {
                if (!(Math.Abs(ears[i].CompareTo(trueval[i]) - 0.0) < tol))
                {
                    throw new System.InvalidOperationException("Error en test 5");
                }
            }

            //Test 6 (valores)
            tol = 0.00001;
            trueval = new bool[] { false, false, true };
            x = new int[] { 0, 0, 100 };
            ears = EARSC1(x, 0.05, 0.0, 2);
            for (int i = 0; i < ears.Length; i++)
            {
                if (!(Math.Abs(ears[i].CompareTo(trueval[i]) - 0.0) < tol))
                {
                    throw new System.InvalidOperationException("Error en test 6");
                }
            }

            //Test 7 (error de pocos casos)
            tol = 0.00001;
            trueval = new bool[] { false, false, true };
            x = new int[] { 0, 0, 100 };
            try
            {
                ears = EARSC1(x, 0.05, 0.0, 3);
                throw new System.InvalidOperationException("Error en test 7");
            }
            catch (SystemException)
            {

            }

            //Test 8 (valores)
            tol = 0.00001;
            x = new int[] { 1, 2, 1, 0, 0, 1, 2, 3, 1, 1, 0, 0, 0, 1, 20, 40, 1, 1, 1, 0, 0, 0, 1, 1 };
            trueval = new bool[x.Length];
            trueval[7] = true;
            trueval[14] = true;
            trueval[15] = true;
            ears = EARSC1(x, 0.05, 0, 7);
            for (int i = 0; i < ears.Length; i++)
            {
                if (!(Math.Abs(ears[i].CompareTo(trueval[i]) - 0.0) < tol))
                {
                    throw new System.InvalidOperationException("Error en test 8");
                }
            }
            return true;

        }

        private bool TestAlerta()
        {
            bool bandera = true;
            Console.WriteLine("Probando Alerta");

            //Test 1 (longitud)
            bool[] alerta = Alerta();
            if (alerta.Length != Casos.Length)
            {
                throw new System.InvalidOperationException("Error en test 1");
            }

            return bandera;
        }

        //Test de todas las funciones
        public bool Test()
        {
            Console.Write("**Tests**\n");
            bool test_result = (TestMedia() & TestMediaMovil() & TestDesviacionEstandar() & TestZval() & TestDesviacionEstandarMovil() & TestEARSC1() & TestAlerta());
            Console.Write("**Tests Finalizados**\n");
            return test_result;
        }

    }
}
