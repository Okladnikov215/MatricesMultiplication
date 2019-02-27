# MatricesMultiplication

This is a project which implements matrices multiplication using "System.Threading.Tasks" library

It has overloaded *operator which does casual sequential multiplication and Matrix.ParallelMultiplyWithTask which utilizes Task class only.
Also there is Matrix.ParallelMultiplyWithFor that uses Parallel.For which seems to be the fastest way to parallel matrices multiplication.
