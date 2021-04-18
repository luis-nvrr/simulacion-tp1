﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Numeros_aleatorios.LibreriaSimulacion.Probadores
{
    class ProbadorPoisson : IProbador
    {

        private DataTable numeros;
        private int[] valoresEnteros;
        private int[][] valoresAgrupados;
        private int[] frecuenciasObservadas;
        private int[] frecuenciasEsperadas;
        private float[] probabilidades;
        private DataTable resultado;
        private Truncador truncador;
        private float valorCritico;
        private double lambda;

        public ProbadorPoisson(Truncador truncador, DataTable numeros,
            double lambda, int[] valoresEnteros, int[] frecuenciasObservadas)
        {
            this.numeros = numeros;
            this.lambda = lambda;
            this.valoresEnteros = valoresEnteros;
            this.frecuenciasObservadas = frecuenciasObservadas;
            this.truncador = truncador;
        }

        private void crearTabla(DataTable tabla)
        {
            tabla.Columns.Add("valores");
            tabla.Columns.Add("FO");
            tabla.Columns.Add("P()");
            tabla.Columns.Add("FE");
            tabla.Columns.Add("C");
            tabla.Columns.Add("C(AC)");
        }

        public void probar()
        {
            construirTablaInicial();
            reestructurarIntervalos();
            construirTablaFinal();
        }

        private void construirTablaInicial()
        {
            this.resultado = new DataTable();
            crearTabla(resultado);
            DataRow row;
            int entero;
            double estadisticoPrueba;
            double funcionDensidad;
            float cantidadNumeros = numeros.Rows.Count;
            double frecuenciaEsperada;

            for (int i = 0; i < frecuenciasObservadas.Length; i++)
            {
                entero = valoresEnteros[i];
                row = resultado.NewRow();
                row[0] = entero;
                row[1] = frecuenciasObservadas[i];

                funcionDensidad = ((Math.Pow(lambda,entero))*Math.Exp(-lambda))/(Factorial.factorial(entero)); // TODO arreglar factorial retorna un entero muy grande
                row[2] = truncador.truncar(funcionDensidad); 

                frecuenciaEsperada = (funcionDensidad * cantidadNumeros);
                row[3] = (int)Math.Round(frecuenciaEsperada,0);
                resultado.Rows.Add(row);

                //MessageBox.Show(frecuenciaEsperada.ToString());
            }
        }

        private Boolean compararEstadisticoConAcumulado()
        {
            int gradosLibertad = calcularGradosLibertad();
            valorCritico = obtenerValorCritico(gradosLibertad);

            if (valorCritico > obtenerTotalAcumuladoEstadisticoPrueba())
            {
                return true;
            }
            return false;
        }

        private int calcularGradosLibertad()
        {
            return valoresAgrupados.Length - 1;
        }

        public bool esAceptado()
        {
            return compararEstadisticoConAcumulado();
        }

        private float obtenerValorCritico(int gradosLibertad)
        {
            return ValorCriticoChi2.obtenerValorCritico(gradosLibertad);
        }

        public DataTable obtenerTablaResultados()
        {
            return resultado;
        }

        public float obtenerTotalAcumuladoEstadisticoPrueba()
        {
            return float.Parse(resultado.Rows[resultado.Rows.Count - 1][5].ToString());
        }

        public float getValorCritico()
        {
            return valorCritico;
        }

        private void reestructurarIntervalos()
        {
            int esperadaTablaVieja;
            int esperadaAcumulada = 0;
            int observadaAcumulada = 0;
            float probabilidadTablaVieja;
            double probabilidadAcumulada = 0;
            List<int> valoresAgruapados = new List<int>();
            List<List<int>> nuevosValoresAgrupados = new List<List<int>>();
            List<int> nuevaFrecuenciaObservada = new List<int>();
            List<int> nuevaFrecuenciaEsperada = new List<int>();
            List<float> nuevaProbabilidad = new List<float>();

            for (int i = 0; i < frecuenciasObservadas.Length; i++)
            {
                valoresAgruapados.Add(valoresEnteros[i]);
                esperadaTablaVieja = int.Parse(resultado.Rows[i][3].ToString());
                esperadaAcumulada += esperadaTablaVieja;
                probabilidadTablaVieja = float.Parse(resultado.Rows[i][2].ToString());
                probabilidadAcumulada += probabilidadTablaVieja;
                observadaAcumulada += frecuenciasObservadas[i];

                if (esperadaAcumulada > 5)
                {
                    nuevosValoresAgrupados.Add(valoresAgruapados);
                    nuevaFrecuenciaObservada.Add(observadaAcumulada);
                    nuevaFrecuenciaEsperada.Add(esperadaAcumulada);
                    nuevaProbabilidad.Add(truncador.truncar(probabilidadAcumulada));

                    probabilidadAcumulada = 0;
                    esperadaAcumulada = 0;
                    observadaAcumulada = 0;
                    valoresAgruapados = new List<int>();
                }
            }

            nuevaFrecuenciaObservada[nuevosValoresAgrupados.Count - 1] += observadaAcumulada;
            nuevaFrecuenciaEsperada[nuevosValoresAgrupados.Count - 1] += esperadaAcumulada;
            nuevaProbabilidad[nuevosValoresAgrupados.Count - 1] = truncador.truncar(nuevaProbabilidad[nuevosValoresAgrupados.Count - 1] + probabilidadAcumulada);

            this.valoresAgrupados = nuevosValoresAgrupados.Select(l => l.ToArray()).ToArray();
            this.frecuenciasObservadas = nuevaFrecuenciaObservada.ToArray();
            this.frecuenciasEsperadas = nuevaFrecuenciaEsperada.ToArray();
            this.probabilidades = nuevaProbabilidad.ToArray();
            //MessageBox.Show(mostrarNuevosIntervalos(nuevoInicioIntervalos, nuevoFinIntervalos));
        }


        private void construirTablaFinal()
        {
            this.resultado = new DataTable();
            crearTabla(resultado);
            DataRow row;
            double estadisticoPrueba;
            double estadisticoPruebaAcumuladoAnterior = 0;
            float marcaClase;
            float frecuenciaEsperada;

            for (int i = 0; i < frecuenciasObservadas.Length; i++)
            {
                row = resultado.NewRow();
                row[0] = "";
                for (int j = 0; j < valoresAgrupados[i].Length; j++)
                {
                    row[0] += valoresAgrupados[i][j].ToString() + ";";
                }

                row[1] = frecuenciasObservadas[i];

                row[2] = probabilidades[i];  // probabilidad

                frecuenciaEsperada = frecuenciasEsperadas[i];
                row[3] = frecuenciaEsperada; // frecuenciaEsperada


                estadisticoPrueba = (Math.Pow((frecuenciaEsperada - frecuenciasObservadas[i]), 2) / frecuenciaEsperada);
                row[4] = truncador.truncar(estadisticoPrueba);
                row[5] = truncador.truncar(estadisticoPruebaAcumuladoAnterior + estadisticoPrueba);
                estadisticoPruebaAcumuladoAnterior += estadisticoPrueba;
                resultado.Rows.Add(row);
            }
        }

        private String mostrarNuevosIntervalos(List<float> inicio, List<float> fin)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < inicio.Count; i++)
            {
                stringBuilder.Append(inicio[i]).Append(" ").Append(fin[i]);
                stringBuilder.Append("\n");
            }
            return stringBuilder.ToString();
        }
    }
}
