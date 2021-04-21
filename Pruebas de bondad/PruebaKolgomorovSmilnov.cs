﻿using Numeros_aleatorios.grafico_excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Numeros_aleatorios
{
    public partial class PruebaKolgomorovSmilnov : Form
    {
        int indice = -1;
        int n;
        int cantidadIntervalos;
        int[] frecuenciaObservada;
        float[] inicioIntervalos;
        float[] finIntervalos;
        Random random;
        DataTable tabla1;
        DataTable tabla2;
        GraficadorExcelObservado graficador;
        
        double esperadaAcumuladaAnterior;
        double observadaAcumuladaAnterior;

        //double[] kolgomorovosSmilnov = { 0, 0.97500, 0.84189, 0.70760, 0.62394, 0.56328, 0.51926, 0.48342, 0.45427,
        //                                0.43001, 0.40925, 0.39122, 0.37543, 0.36143, 0.34890, 0.33750, 0.32733, 0.31796,
        //                                0.30936, 0.30143, 0.29408, 0.28724, 0.28087, 0.27490, 0.26931, 0.26404, 0.25908,
        //                                0.25438, 0.24933, 0.24571, 0.24170, 0.23788, 0.23424, 0.23076, 0.22743, 0.22425};
        public PruebaKolgomorovSmilnov()
        {
            InitializeComponent();
        }

        private void btnGenerar_Click(object sender, EventArgs e)
        {
            tomarEntrada();
            if (n <= 0) { MessageBox.Show("El tamaño de la muestra debe ser mayor a 0..."); }
            if (n > 0)
            {
                generarNumerosAleatorios();
                evaluarHipotesis();
            }
        }

        private float truncarDecimales(double numero)
        {
            int factor = 10000;
            return (float)Math.Truncate(factor * numero) / factor;
        }
        private double max(double a,double b)
        {
            if (a >= b)
            {
                return a;
            }
            return b;
        }



        private void generarNumerosAleatorios()
        {
            tabla1.Rows.Clear();
            tabla2.Rows.Clear();
            
            indice = -1;

            frecuenciaObservada = new int[cantidadIntervalos];

            double longitudIntervalo = 1.0f / frecuenciaObservada.Length;
            MessageBox.Show(frecuenciaObservada.Length.ToString());
            float inicioIntervalo;
            float finIntervalo;

            string intervalo;
            

            float probabilidadEsperada = truncarDecimales(1.0f / cantidadIntervalos);
            float probabilidadObservada;
            float frecuenciaEsperada = n / cantidadIntervalos ;

            double maximo = 0;
            // genera aleatorios y se fija en que intervalo pertenece

            DataRow filatabla1;
            DataRow filatabla2;

            inicioIntervalos = new float[cantidadIntervalos];
            finIntervalos = new float[cantidadIntervalos];


            for (int i = 0; i < cantidadIntervalos; i++)
            {
                filatabla2 = tabla2.NewRow();
                tabla2.Rows.Add(filatabla2);                
            }

            for (int i = 0; i < n; i++)
            {
                float truncado = truncarDecimales(random.NextDouble());

                filatabla1 = tabla1.NewRow(); 
                filatabla1[0] = i + 1;
                filatabla1[1] = truncado;
                tabla1.Rows.Add(filatabla1);
                
                for (int j = 0; j < frecuenciaObservada.Length; j++)
                {
                    inicioIntervalo = truncarDecimales(longitudIntervalo * j);
                    finIntervalo = truncarDecimales(longitudIntervalo * (1 + j) - 0.0001f);
                    inicioIntervalos[j] = inicioIntervalo;
                    finIntervalos[j] = finIntervalo;

                    if (truncado >= inicioIntervalo &&
                           truncado <= finIntervalo)
                    {
                        intervalo = "[" + inicioIntervalo + "; " + finIntervalo + "]";
                        frecuenciaObservada[j] += 1;

                        probabilidadObservada = truncarDecimales((double)frecuenciaObservada[j] / n);                        
                        // agrega fila y columnas de frecuencias esperadas y observadas

                        tabla2.Rows[j][0] = intervalo;
                        tabla2.Rows[j][1] = frecuenciaObservada[j];
                        tabla2.Rows[j][2] = frecuenciaEsperada;
                        tabla2.Rows[j][3] = probabilidadObservada;
                        tabla2.Rows[j][4] = probabilidadEsperada;
                        break;
                    } 
                }
            }

            for (int i = 0; i < cantidadIntervalos; i++)
            {
                esperadaAcumuladaAnterior = 0;
                observadaAcumuladaAnterior = 0;

                if (i != 0 && tabla2.Rows[i - 1][6].ToString() != "")
                {
                    esperadaAcumuladaAnterior = double.Parse(tabla2.Rows[i - 1][6].ToString());

                }
                if (i != 0 && tabla2.Rows[i - 1][5].ToString() != "")
                {
                    observadaAcumuladaAnterior = double.Parse(tabla2.Rows[i - 1][5].ToString());
                }

                tabla2.Rows[i][5] = truncarDecimales(double.Parse(tabla2.Rows[i][3].ToString()) + observadaAcumuladaAnterior);
                tabla2.Rows[i][6] = truncarDecimales(double.Parse(tabla2.Rows[i][4].ToString()) + esperadaAcumuladaAnterior);
                double diferencia = Math.Abs(double.Parse(tabla2.Rows[i][5].ToString()) - double.Parse(tabla2.Rows[i][6].ToString()));
                tabla2.Rows[i][7] = truncarDecimales(diferencia);
            }

            tabla2.Rows[0][8] = tabla2.Rows[0][7];

            for (int i = 1; i < cantidadIntervalos; i++)
            {
                maximo = max(double.Parse(tabla2.Rows[i - 1][8].ToString()), double.Parse(tabla2.Rows[i][7].ToString()));
                tabla2.Rows[i][8] = truncarDecimales(maximo);
            }

            grdResultados.DataSource = tabla1;
            grdResultados2.DataSource = tabla2;
        }

        public void mostrarGrafico()
        {
            graficador = new GraficadorExcelObservado();
            graficador.frecuenciaObservada = frecuenciaObservada;
           //graficador.nombre = gbDistribuciones.Controls.OfType<RadioButton>().FirstOrDefault(n => n.Checked).Text;
            graficador.inicioIntervalos = this.inicioIntervalos;
            graficador.finIntervalos = this.finIntervalos;
            graficador.Show();
        }

        public void evaluarHipotesis()
        {
            txtGradosLibertad.Text = n.ToString();
            double tabulado = truncarDecimales(1.36f / Math.Sqrt(n));
            txtProbabilidad.Text = tabulado.ToString();
            if (double.Parse(tabla2.Rows[cantidadIntervalos - 1][8].ToString()) <= tabulado)
            {
                lblResultadoHipotesis.Text = "Con un nivel de significancia de 0,05 NO se rechaza la hipotesis nula";
            }
            else
            {
                lblResultadoHipotesis.Text = "Con un nivel de significancia de 0,05 se rechaza la hipotesis nula";
            }
        }

        private void PruebaKolgomorovSmilnov_Load(object sender, EventArgs e)
        {
            tabla1 = new DataTable();
            tabla2 = new DataTable();
            random = new Random();
            tabla1.Columns.Add("Iteración");
            tabla1.Columns.Add("Número Aleatorio");
            tabla2.Columns.Add("Intervalo");
            tabla2.Columns.Add("Frecuencia Observada");
            tabla2.Columns.Add("Frecuencia Esperada");
            tabla2.Columns.Add("PO");
            tabla2.Columns.Add("PE");
            tabla2.Columns.Add("PO(ac)");
            tabla2.Columns.Add("PE(ac)");
            tabla2.Columns.Add("| PoAC - PeAC |");
            tabla2.Columns.Add("max(| PoAC - PeAC |)");

        }
        private void tomarEntrada()
        {
            n = int.Parse(tamanioMuestra.Text);
            if (rb5.Checked) { cantidadIntervalos = int.Parse(rb5.Text); }
            if (rb10.Checked) { cantidadIntervalos = int.Parse(rb10.Text); }
            if (rb15.Checked) { cantidadIntervalos = int.Parse(rb15.Text); }
            if (rb20.Checked) { cantidadIntervalos = int.Parse(rb20.Text); }
        }

        private String tabla1ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (DataRow row in tabla1.Rows)
            {
                stringBuilder.Append(row[0].ToString()).Append("\t").Append(row[1].ToString());
                stringBuilder.Append("\n");
            }
            return stringBuilder.ToString();
        }
        private String tabla2ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (DataRow row in tabla2.Rows)
            {
                stringBuilder.Append(row[0].ToString()).Append("\t").Append(row[1].ToString()).Append("\t").Append(row[2].ToString()).Append("\t").Append(row[3].ToString()).Append("\t").Append(row[4].ToString()).Append("\t").Append(row[5].ToString()).Append("\t").Append(row[6].ToString()).Append("\t").Append(row[7].ToString()).Append("\t").Append(row[8].ToString());
                stringBuilder.Append("\n");
            }
            return stringBuilder.ToString();
        }

        private void btnCopiar_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(tabla1ToString());
            MessageBox.Show("Texto copiado!", "Clipboard", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(tabla2ToString());
            MessageBox.Show("Texto copiado!", "Clipboard", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnMostrar_Click(object sender, EventArgs e)
        {
            mostrarGrafico();
        }

        private void btnProbar_Click(object sender, EventArgs e)
        {
            evaluarHipotesis();
        }
    }
}