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
    class MainClass
    {
        public static void Main(string[] args)
        {
            //Ejemplo el siguiente array contiene la cantidad de casos
            //de enfermedad respiratoria registrados en una empresa
            //por semana epidemiológica. El array debe incluir por lo menos
            //las últimas 7 semanas aunque se recomienda (para generalizar)
            //tomar desde la primer semana del 2020.
            int[] casos_respiratorios = { 1, 2, 1, 0, 1, 3, 2, 3 };

            //El constructor crea una nueva empresa con dicho array
            //Hay dos constructores distintos el que se usaría de manera general
            //aunque se cambie el método de alerta es este.
            var empresa = new CasosEmpresa(casos_respiratorios);

            //El sistema de alertas regresa un array del mismo tamaño que
            //el vector inicial de casos_respiratorios con las alertas correspondientes.
            bool[] alertas = empresa.Alerta();

            //Aquí imprimimos las alertas nada más para verlas
            Console.WriteLine("Alerta!");
            for (int i = 0; i < alertas.Length; i++)
            {
                Console.WriteLine($"Semana {i + 1} emite alerta = {alertas[i]} por {casos_respiratorios[i]} casos");
            }

            //La función test es sólo para checar que todo esté funcionando bien
            //se puede comentar porque no tiene propósito más allá del debugging
           empresa.Test();
        }
    }
}
