//using Google.OrTools.LinearSolver;
using Google.OrTools.ConstraintSolver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestMethod
{
    class Program
    {
        private static BlockingCollection<int> queue = new BlockingCollection<int>();
        static void Main(string[] args)
        {
            //Solve();
            Test();
            //TestLinearSolver();
            Console.ReadLine();
        }

        //private static void TestLinearSolver()
        //{
        //    Solver solver = new Solver("LeastDiff", Solver.CLP_LINEAR_PROGRAMMING);
        //    Variable r1 = solver.MakeNumVar(0, 60, "R1");
        //    Variable r2 = solver.MakeNumVar(0, 40, "R2");
        //    Variable r3 = solver.MakeNumVar(0, 40, "R3");
        //    Variable r4 = solver.MakeNumVar(0, 40, "R4");

        //    solver.Add(r1 + r3 + r4 <= 100);
        //    solver.Add(r1 + r2 + r4 <= 80);
        //    solver.Add(r1 + r2 + r3 <= 60);
        //    solver.Add(r2 + r3 + r4 <= 40);

        //    solver.Maximize(r1 + r2 + r3 + r4);
        //    int resultStatus = solver.Solve();
        //    double d1 = r1.SolutionValue();
        //    double d2 = r2.SolutionValue();
        //    double d3 = r3.SolutionValue();
        //    double d4 = r4.SolutionValue();
        //    double a = solver.Objective().Value();
        //}

        private static void Test()
        {
            Solver solver = new Solver("AtLeastCount");
            IntVar r1 = solver.MakeIntVar(0, 60, "R1");
            IntVar r2 = solver.MakeIntVar(0, 40, "R2");
            IntVar r3 = solver.MakeIntVar(0, 40, "R3");
            IntVar r4 = solver.MakeIntVar(0, 40, "R4");

            solver.Add(r1 + r3 + r4 <= 100);
            solver.Add(r1 + r2 + r4 <= 80);
            solver.Add(r1 + r2 + r3 <= 60);
            solver.Add(r2 + r3 + r4 <= 40);

            //IntVarVector v = new IntVarVector();
            //v.Add(r1);
            //v.Add(r2);
            //v.Add(r3);
            //v.Add(r4);

            //IntExpr exp = solver.MakeSum(v);

            //solver.Add(exp >= 89);
            //solver.Add(solver.MakeSumGreaterOrEqual(v, 88));

            IntVar temp1 = solver.MakeGreaterOrEqual(r1 + r3 + r4, 100);
            IntVar temp2 = solver.MakeGreaterOrEqual(r1 + r2 + r4, 80);
            IntVar temp3 = solver.MakeGreaterOrEqual(r1 + r2 + r3, 60);
            solver.Add(temp1 + temp2 + temp3 > 0);

            temp1 = solver.MakeGreaterOrEqual(r1 + r2 + r4, 80);
            temp2 = solver.MakeGreaterOrEqual(r1 + r2 + r3, 60);
            temp3 = solver.MakeGreaterOrEqual(r2 + r3 + r4, 40);
            solver.Add(temp1 + temp2 + temp3 > 0);

            temp1 = solver.MakeGreaterOrEqual(r1 + r3 + r4, 100);
            temp2 = solver.MakeGreaterOrEqual(r1 + r2 + r3, 60);
            temp3 = solver.MakeGreaterOrEqual(r2 + r3 + r4, 40);
            solver.Add(temp1 + temp2 + temp3 > 0);

            temp1 = solver.MakeGreaterOrEqual(r1 + r3 + r4, 100);
            temp2 = solver.MakeGreaterOrEqual(r1 + r2 + r4, 80);
            temp3 = solver.MakeGreaterOrEqual(r2 + r3 + r4, 40);
            solver.Add(temp1 + temp2 + temp3 > 0);

            //IntExpr sum = solver.MakeSum(v);
            IntVar[] vs = new IntVar[] { r1, r2, r3, r4 };
            IntVar z = vs.Sum().Var();

            DecisionBuilder db = solver.MakePhase(vs, Solver.INT_VAR_DEFAULT, Solver.INT_VALUE_DEFAULT);
            //solver.Solve(db, z.Maximize(1));
            solver.NewSearch(db, z.Maximize(1));
            while (solver.NextSolution())
            {
                Console.WriteLine(vs[0].Value());
                Console.WriteLine(vs[1].Value());
                Console.WriteLine(vs[2].Value());
                Console.WriteLine(vs[3].Value());
                Console.WriteLine("");
            }
            long c = solver.Solutions();
            Console.WriteLine(c);
        }

        private static void Solve()
        {

            Solver solver = new Solver("SetCoveringDeployment");

            //
            // data
            //

            // From http://mathworld.wolfram.com/SetCoveringDeployment.html
            string[] countries = {"Alexandria",
                          "Asia Minor",
                          "Britain",
                          "Byzantium",
                          "Gaul",
                          "Iberia",
                          "Rome",
                          "Tunis"};

            int n = countries.Length;

            // the incidence matrix (neighbours)
            int[,] mat = {{0, 1, 0, 1, 0, 0, 1, 1},
                  {1, 0, 0, 1, 0, 0, 0, 0},
                  {0, 0, 0, 0, 1, 1, 0, 0},
                  {1, 1, 0, 0, 0, 0, 1, 0},
                  {0, 0, 1, 0, 0, 1, 1, 0},
                  {0, 0, 1, 0, 1, 0, 1, 1},
                  {1, 0, 0, 1, 1, 1, 0, 1},
                  {1, 0, 0, 0, 0, 1, 1, 0}};

            //
            // Decision variables
            //

            // First army
            IntVar[] x = solver.MakeIntVarArray(n, 0, 1, "x");

            // Second (reserve) army
            IntVar[] y = solver.MakeIntVarArray(n, 0, 1, "y");

            // total number of armies
            IntVar num_armies = (x.Sum() + y.Sum()).Var();


            //
            // Constraints
            //

            //
            //  Constraint 1: There is always an army in a city
            //                (+ maybe a backup)
            //                Or rather: Is there a backup, there
            //                must be an an army
            //
            for (int i = 0; i < n; i++)
            {
                solver.Add(x[i] >= y[i]);
            }

            //
            // Constraint 2: There should always be an backup
            //               army near every city
            //
            for (int i = 0; i < n; i++)
            {
                IntVar[] count_neighbours = (
                                             from j in Enumerable.Range(0, n)
                                             where mat[i, j] == 1
                                             select (y[j])).ToArray();

                solver.Add((x[i] + count_neighbours.Sum()) >= 1);

            }


            //
            // objective
            //
            OptimizeVar objective = num_armies.Minimize(1);


            //
            // Search
            //
            DecisionBuilder db = solver.MakePhase(x,
                                                  Solver.INT_VAR_DEFAULT,
                                                  Solver.INT_VALUE_DEFAULT);

            solver.NewSearch(db, objective);

            while (solver.NextSolution())
            {
                Console.WriteLine("num_armies: " + num_armies.Value());
                for (int i = 0; i < n; i++)
                {
                    if (x[i].Value() == 1)
                    {
                        Console.Write("Army: " + countries[i] + " ");
                    }

                    if (y[i].Value() == 1)
                    {
                        Console.WriteLine(" Reverse army: " + countries[i]);
                    }
                }
                Console.WriteLine("\n");

            }

            Console.WriteLine("\nSolutions: {0}", solver.Solutions());
            Console.WriteLine("WallTime: {0}ms", solver.WallTime());
            Console.WriteLine("Failures: {0}", solver.Failures());
            Console.WriteLine("Branches: {0} ", solver.Branches());

            solver.EndSearch();

        }
    }
}
