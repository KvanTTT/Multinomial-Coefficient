Multinomial-Coefficient
========================

A library for <a ref="http://en.wikipedia.org/wiki/Multinomial_theorem#Multinomial_coefficients">multinominal coefficient</a> calculating in different ways:

- BigInteger.
- Decomposion on binominal coefficients multiplication.
- Logarithms method.
- Logarithms of Factorial method.
- My algorithm.
- Decomposion on prime numbers.

Also with library is possible to compute coefficients and summands for polynomial decomposition.
For this example:
```
(a + b + c + d) ^ 3 = a^3 + 
3*a^2*b + 3*a^2*c + 3*a^2*d + 3*a*b^2 + 
6*a*b*c + 6*a*b*d + 3*a*c^2 + 6*a*c*d + 3*a*d^2 +
b^3 + 3*b^2*c + 3*b^2*d + 3*b*c^2 + 6*b*c*d + 
3*b*d^2 + c^3 + 3*c^2*d + 3*c*d^2 + d^3
```

We get can get following factors and coefs (0 - 'a', 1 - 'b', 2 - 'c', 3 - 'd') with
```CombinationWithRepetition.GenerateSummands(n, k)``` method:

Factors | Coef 
------|-------
0,0,0 | 1
0,0,1 | 3
0,0,2 | 3
0,0,3 | 3
0,1,1 | 3
0,1,2 | 6
0,1,3 | 6
0,2,2 | 3
0,2,3 | 6
0,3,3 | 3
1,1,1 | 1
1,1,2 | 3
1,1,3 | 3
1,2,2 | 3
1,2,3 | 6
1,3,3 | 3
2,2,2 | 1
2,2,3 | 3
2,3,3 | 3
3,3,3 | 1
