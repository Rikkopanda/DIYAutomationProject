using System;
using System.Globalization;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace forwardkina
{
	class forward
    {
        double LiniearYtravel = 200;
        int matrixsize = 3;
        double[] ZeroTo1 = { 0, LiniearYtravel, 0 };
        double[] Oneto2 = { 0, 0, 100 };
        double TwoTo3 = 350;
        double ThreeTo4 = 350;
        double theta1;
        double theta2;
        double theta3;
        double theta1rad;
        double theta2rad;
        double theta3rad;
        Matrix<double> R12;
        int ForwardCalculate(string T1, string T1, string T1)
        {
            theta1 = Convert.ToDouble(T1);
            theta2 = Convert.ToDouble(T2);
            theta3 = Convert.ToDouble(T3);
            theta1rad = Math.PI * theta1 / 180.0;
            theta2rad = Math.PI * theta2 / 180.0;
            theta3rad = Math.PI * theta3 / 180.0;

            Matrix<double> I = Matrix<double>.Build.DenseOfArray(new double[matrixsize, matrixsize]);//identity matrix
            I[0, 0] = 1;
            I[1, 1] = 1;
            I[2, 2] = 1;
            Matrix<double> M1 = Matrix<double>.Build.DenseOfArray(new double[matrixsize, matrixsize]);//flip coordinate for 1-2
            M1[0, 0] = 1;
            M1[2, 1] = 1;
            M1[1, 2] = -1;
            //---------
            //individual Rotation and displacement matrixes
            R12 = Matrix<double>.Build.DenseOfArray(new double[3, 3]
            { {Math.Cos(theta1rad),Math.Sin(theta1rad),0 },{ Math.Sin(theta1rad),Math.Cos(theta1rad),0 },{ 0,0,1 } });
            Matrix<double> R23 = Matrix<double>.Build.DenseOfArray(new double[3, 3]
            { {Math.Cos(theta2rad),Math.Sin(theta2rad),0 },{ Math.Sin(theta2rad),Math.Cos(theta2rad),0 },{ 0,0,1 } });
            Matrix<double> R34 = Matrix<double>.Build.DenseOfArray(new double[3, 3]
            { {Math.Cos(theta3rad),Math.Sin(theta3rad),0 },{ Math.Sin(theta3rad),Math.Cos(theta3rad),0 },{ 0,0,1 } });

            double[] Displace01 = { 0, LiniearYtravel, 0 };
            double[] Displace12 = { Oneto2[0] * Math.Cos(theta1rad), Oneto2[1] * Math.Sin(theta1rad), Oneto2[2] };//x,    y,    z
            double[] Displace23 = { TwoTo3 * Math.Cos(theta2rad), TwoTo3 * Math.Sin(theta2rad), 0 };
            double[] Displace34 = { ThreeTo4 * Math.Cos(theta3rad), ThreeTo4 * Math.Sin(theta3rad), 0 };
            //---------

            print_MatNet_matrix(M1, "fflip1\n");
            R12 = R12 * M1;
            print_MatNet_matrix(R12, "Matrix R12\n");
            print_MatNet_matrix(R23, "Matrix R23\n");
            print_MatNet_matrix(R34, "Matrix R34\n");
            Console.WriteLine("\nd01");
            for (int i = 0; i < 3; i++)
                Console.WriteLine(Displace01[i]);
            Console.WriteLine("\nd12");
            for (int i = 0; i < 3; i++)
                Console.WriteLine(Displace12[i]);
            Console.WriteLine("\nd23");
            for (int i = 0; i < 3; i++)
                Console.WriteLine(Displace23[i]);
            Console.WriteLine("\nd34");
            for (int i = 0; i < 3; i++)
                Console.WriteLine(Displace34[i]);

            //homogeneneous T matrixes
            Matrix<double> H01 = Matrix<double>.Build.DenseOfArray(new double[4, 4]);
            Matrix<double> H12 = Matrix<double>.Build.DenseOfArray(new double[4, 4]);
            Matrix<double> H23 = Matrix<double>.Build.DenseOfArray(new double[4, 4]);
            Matrix<double> H34 = Matrix<double>.Build.DenseOfArray(new double[4, 4]);
            Matrix<double> H02 = Matrix<double>.Build.DenseOfArray(new double[4, 4]);
            Matrix<double> H03 = Matrix<double>.Build.DenseOfArray(new double[4, 4]);
            Matrix<double> H04 = Matrix<double>.Build.DenseOfArray(new double[4, 4]);

            make_Homogen_matrix(H01, I, Displace01);
            make_Homogen_matrix(H12, R12, Displace12);
            make_Homogen_matrix(H23, R23, Displace23);
            make_Homogen_matrix(H34, R34, Displace34);

            print_MatNet_matrix(H01, "Matrix H01\n");
            print_MatNet_matrix(H12, "Matrix H12\n");
            print_MatNet_matrix(H23, "Matrix H23\n");
            print_MatNet_matrix(H34, "Matrix H34\n");
            H02 = H01 * H12;
            H03 = H02 * H23;
            H04 = H03 * H34;
            round_matrix_numbers(H04, 3);
            print_MatNet_matrix(H04, "Matrix H04\n");
        }

        void round_matrix_numbers(Matrix<double> matrix, int round_after_deci)
        {
            int rows = matrix.RowCount;
            int cols = matrix.ColumnCount;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                    matrix[i, j] = Math.Round(matrix[i, j], round_after_deci);
            }
        }

        void make_Homogen_matrix(Matrix<double> Homogen, Matrix<double> Rota, double[] Discplace)
        {
            if (Homogen.RowCount != 4 || Homogen.RowCount != 4)
            {
                Console.WriteLine("wrong input in Make_Homogen_matrix");
                return;

            }
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (j < 3 && i < 3)
                        Homogen[i, j] = Rota[i, j];
                    else if (i < 3)
                        Homogen[i, j] = Discplace[i];
                }
            }
            Homogen[3, 3] = 1;
        }

        void print_MatNet_matrix(Matrix<double> ja, string tekst)
        {
            int rows = ja.RowCount;
            int cols = ja.ColumnCount;
            Console.Write(tekst);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < rows; j++)
                    Console.Write(ja[i, j] + "\t");
                Console.Write("\n");
            }
        }
    }
}
