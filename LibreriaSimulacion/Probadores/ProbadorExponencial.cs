﻿using Numeros_aleatorios.LibreriaSimulacion.GeneradoresIntervalos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Numeros_aleatorios.LibreriaSimulacion.Probadores
{
    class ProbadorExponencial : IProbador
    {
        private DataTable numeros;
        private float[] inicioIntervalos;

        private float[] finIntervalos;
        private int[] frecuenciasObservadas;
        private float[] frecuenciasEsperadas;
        private float[] probabilidades;
        private DataTable resultado;
        private Truncador truncador;
        private float valorCritico;
        private double lambda;
        private double media;

        public ProbadorExponencial(Truncador truncador, DataTable numeros,
                                   double media, double lambda,
                                   int cantidadIntervalos, float[] inicioIntervalos, float[] finIntervalos, int[] frecuenciasObservadas)
        {
            this.numeros = numeros;
            this.truncador = truncador;
            this.media = media;
            this.lambda = lambda;
            this.inicioIntervalos = inicioIntervalos;
            this.finIntervalos = finIntervalos;
            this.frecuenciasObservadas = frecuenciasObservadas;
        }

        private void crearTabla(DataTable tabla)
        {
            resultado.Columns.Add("intervalo");
            resultado.Columns.Add("MC");
            resultado.Columns.Add("FO");
            resultado.Columns.Add("P(x)");
            resultado.Columns.Add("FE");
            resultado.Columns.Add("C");
            resultado.Columns.Add("C(AC)");
        }

        public bool esAceptado()
        {
            return compararEstadisticoConAcumulado();
        }

        public float getValorCritico()
        {
            return valorCritico;
        }
        private int calcularGradosLibertad()
        {
            return inicioIntervalos.Length - 1;
        }

        public DataTable obtenerTablaResultados()
        {
            return resultado;
        }

        public float obtenerTotalAcumuladoEstadisticoPrueba()
        {
            return float.Parse(resultado.Rows[resultado.Rows.Count - 2][6].ToString());
        }

        public void probar()
        {
            construirTabla();
            reestructurarTabla();
            construirTablaFinal();
            agregarTotalObservada();
        }

        private void construirTabla()
        {
            this.resultado = new DataTable();
            crearTabla(resultado);
            DataRow row;
            float marcaClase;
            double probabilidad;
            float cantidadNumeros = numeros.Rows.Count;
            double frecuenciaEsperada;

            for (int i = 0; i < inicioIntervalos.Length; i++)
            {
                row = resultado.NewRow();
                row[0] = "[" + inicioIntervalos[i] + ";" + finIntervalos[i] + "]";

                marcaClase = truncador.truncar((inicioIntervalos[i] + finIntervalos[i]) / 2.0f);
                row[1] = marcaClase;

                row[2] = frecuenciasObservadas[i];

                probabilidad = (1 - Math.Exp(-lambda * finIntervalos[i])) - (1 - Math.Exp(-lambda * inicioIntervalos[i]));
                row[3] = truncador.truncar(probabilidad);

                frecuenciaEsperada = probabilidad * cantidadNumeros;
                row[4] = truncador.truncar(frecuenciaEsperada);

                resultado.Rows.Add(row); //Agrega una fila completa


            }
        }
        private float obtenerValorCritico(int gradosLibertad)
        {
            return ValorCriticoChi2.obtenerValorCritico(gradosLibertad);
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

        public int[] getFrecuenciasObservadas()
        {
            return this.frecuenciasObservadas;
        }

        public float[] getInicioIntervalos()
        {
            return this.inicioIntervalos;
        }

        public float[] getFinIntervalos()
        {
            return this.finIntervalos;
        }

        private void reestructurarTabla()
        {
            float esperadaTablaVieja;
            double esperadaAcumulada = 0;
            int observadaAcumulada = 0;
            float probabilidadTablaVieja;
            double probabilidadAcumulada = 0;
            List<float> nuevoInicioIntervalos = new List<float>();
            List<float> nuevoFinIntervalos = new List<float>();
            List<int> nuevaFrecuenciaObservada = new List<int>();
            List<float> nuevaFrecuenciaEsperada = new List<float>();
            List<float> nuevaProbabilidad = new List<float>();
            float nuevoInicioIntervalo = 0;
            float nuevoFinIntervalo = 0;

            for (int i = 0; i < inicioIntervalos.Length; i++)
            {
                if (esperadaAcumulada == 0) { nuevoInicioIntervalo = inicioIntervalos[i]; }
                esperadaTablaVieja = float.Parse(resultado.Rows[i][4].ToString());
                esperadaAcumulada += esperadaTablaVieja;
                probabilidadTablaVieja = float.Parse(resultado.Rows[i][3].ToString());
                probabilidadAcumulada += probabilidadTablaVieja;
                observadaAcumulada += frecuenciasObservadas[i];

                if (esperadaAcumulada > 5)
                {
                    nuevoFinIntervalo = finIntervalos[i];

                    nuevoInicioIntervalos.Add(nuevoInicioIntervalo);
                    nuevoFinIntervalos.Add(nuevoFinIntervalo);
                    nuevaFrecuenciaObservada.Add(observadaAcumulada);
                    nuevaFrecuenciaEsperada.Add(truncador.truncar(esperadaAcumulada));
                    nuevaProbabilidad.Add(truncador.truncar(probabilidadAcumulada));

                    probabilidadAcumulada = 0;
                    esperadaAcumulada = 0;
                    observadaAcumulada = 0;
                }
            }

            nuevoFinIntervalos[nuevoFinIntervalos.Count - 1] = finIntervalos[inicioIntervalos.Length - 1];
            nuevaFrecuenciaObservada[nuevoFinIntervalos.Count - 1] += observadaAcumulada;
            nuevaFrecuenciaEsperada[nuevoFinIntervalos.Count - 1] = truncador.truncar(esperadaAcumulada + nuevaFrecuenciaEsperada[nuevoFinIntervalos.Count - 1]);
            nuevaProbabilidad[nuevoFinIntervalos.Count - 1] = truncador.truncar(nuevaProbabilidad[nuevoFinIntervalos.Count - 1] + probabilidadAcumulada);

            this.inicioIntervalos = nuevoInicioIntervalos.ToArray();
            this.finIntervalos = nuevoFinIntervalos.ToArray();
            this.frecuenciasObservadas = nuevaFrecuenciaObservada.ToArray();
            this.frecuenciasEsperadas = nuevaFrecuenciaEsperada.ToArray();
            this.probabilidades = nuevaProbabilidad.ToArray();
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

            for (int i = 0; i < inicioIntervalos.Length; i++)
            {
                row = resultado.NewRow();
                row[0] = "[" + inicioIntervalos[i] + ";" + finIntervalos[i] + "]";

                marcaClase = truncador.truncar((inicioIntervalos[i] + finIntervalos[i]) / 2.0f);
                row[1] = marcaClase;

                row[2] = frecuenciasObservadas[i];

                row[3] = probabilidades[i];  // probabilidad

                frecuenciaEsperada = frecuenciasEsperadas[i];
                row[4] = frecuenciaEsperada; // frecuenciaEsperada

                estadisticoPrueba = (Math.Pow((frecuenciaEsperada - frecuenciasObservadas[i]), 2) / frecuenciaEsperada);
                row[5] = truncador.truncar(estadisticoPrueba);
                row[6] = truncador.truncar(estadisticoPruebaAcumuladoAnterior + estadisticoPrueba);
                estadisticoPruebaAcumuladoAnterior += estadisticoPrueba;
                resultado.Rows.Add(row);
            }
        }

        private void agregarTotalObservada()
        {
            int acum = 0;
            for (int i = 0; i < frecuenciasObservadas.Length; i++)
            {
                acum += frecuenciasObservadas[i];
            }
            DataRow row = resultado.NewRow();
            row[2] = acum;
            resultado.Rows.Add(row);
        }

    }
}
