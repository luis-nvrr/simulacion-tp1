﻿using Numeros_aleatorios.LibreriaSimulacion.GeneradoresAleatorios;

namespace Numeros_aleatorios.LibreriaSimulacion
{
    class GeneradorCongruencialMultiplicativo : IGenerador
    {
        // para la cantidad de decimales de los aleatorios
        private Truncador truncador;

        private long entradaAnterior;
        private long entradaActual;
        private double aleatorioActual;

        // parametros
        private int a;
        private long m;

        public GeneradorCongruencialMultiplicativo(Truncador truncador, long semilla, int a, long m)
        {
            this.entradaAnterior = semilla;
            this.truncador = truncador;
            this.a = a;
            this.m = m;
        }

        // retorna un aleatorio
        public float siguienteAleatorio()
        {
            entradaActual = (a * entradaAnterior) % (m);
            aleatorioActual = (double)entradaActual / (m - 1); // (m-1) para incluir el 1 
            entradaAnterior = entradaActual;
            return truncador.truncar(aleatorioActual);
        }

        public float[] generarSerie(int cantidadAleatorios)
        {
            float[] serieAleatorios = new float[cantidadAleatorios];
            for (int i = 0; i < cantidadAleatorios; i++)
            {
                float aleatorio = siguienteAleatorio();
                serieAleatorios[i] = aleatorio;
            }
            return serieAleatorios;
        }
    }
}