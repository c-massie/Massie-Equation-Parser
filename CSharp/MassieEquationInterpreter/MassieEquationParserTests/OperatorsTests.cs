using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Scot.Massie.EquationParser.Tests
{
    public class OperatorsTests
    {
        /*
        
            Where:
                5 represents literals
                +, *, # represent any possible operator symbols (different dummies represent different symbols)
                operator next to something without a space represents a unary prefix or postfix operator
                operator between two things with spaces represents an infix operator symbol
        
            Equations to test:
            
                Unary:
                    +5
                    5+
                    +5+ (left associative)
                    +5+ (right associative)
                    +5* (left associative, same precedence)
                    +5* (left associative, left higher precedence precedence)
                    +5* (left associative, right higher precedence precedence)
                    +5* (right associative, same precedence)
                    +5* (right associative, left higher precedence precedence)
                    +5* (right associative, right higher precedence precedence)
                    
                Chained unary:
                    5++
                        + only
                        + and ++
                    5+*
                    +5*#
                        Precedences (higher to lower, square brackets represent equal precedence):
                            [+, *, #]
                            +, [*, #]
                            *, [+, #]
                            #, [+, *]
                            [+, #], *
                            [*, #], +
                            [+, *], #
                            +, #, *
                            +, *, #
                            #, +, *
                            #, *, +
                            *, #, +
                            *, +, #
                        
                
                Binary:
                    5 + 5
                    
                Chained binary:
                    5 + 5 + 5 (left associative)
                    5 + 5 + 5 (right associative)
                    5 + 5 * 5 (left associative, same precedence)
                    5 + 5 * 5 (left associative, left higher precedence)
                    5 + 5 * 5 (left associative, right higher precedence)
                    5 + 5 * 5 (right associative, same precedence)
                    5 + 5 * 5 (right associative, left higher precedence)
                    5 + 5 * 5 (right associative, right higher precedence)
        
                Unary & binary:
                    *5 + 5 (unary precedence > binary)
                    *5 + 5 (unary precedence < binary)
                    *5 + 5 (unary precedence == binary, unary precedence should be treated as having higher precedence)
                    5 + 5* (unary precedence > binary)
                    5 + 5* (unary precedence < binary)
                    5 + 5* (unary precedence == binary, unary precedence should be treated as having higher precedence)
                    5* + 5 (unary precedence > binary)
                    5* + 5 (unary precedence < binary)
                    5* + 5 (unary precedence == binary, unary precedence should be treated as having higher precedence)
                    5 + *5 (unary precedence > binary)
                    5 + *5 (unary precedence < binary)
                    5 + *5 (unary precedence == binary, unary precedence should be treated as having higher precedence)
                    
                Unary or binary
                    a+*b (postfix +, prefix *, infix +, infix *, infix +*) (left and right associative)
                    
               Chained ternary
                   5 * 5 + 5 * 5 + 5 (left associative)
                   5 * 5 + 5 * 5 + 5 (right associative)
                    
                Edge cases around how n-ary operators are parsed
                    
                    5 +* 5 # 5
                        [+*, #], [*, #], same precedence, left associative
                        [+*, #], [*, #], longer is higher precedence, left associative
                        [+*, #], [*, #], longer is lower precedence, left associative
                        [+*, #], [*, #], same precedence, right associative
                        [+*, #], [*, #], longer is higher precedence, right associative
                        [+*, #], [*, #], longer is lower precedence, right associative
                        [+*, #], [*, #], +x, same precedence, left associative
                        [+*, #], [*, #], +x, same precedence, right associative
                        [+*, #], [*, #], +x, unary has higher precedence
                        [+*, #], [*, #], +x, unary has lower precedence
                        
                    5 + 5 *# 5
                        Inverse of the above
         */
        
        [Fact]
        public void UnaryPrefix()
        {
            var eqp    = new EquationParser().WithPrefixOperator("*", x => x * 5);
            var eq     = eqp.Parse("*7");
            var result = eq.Evaluate();

            result.Should().Be(35);
        }

        [Fact]
        public void UnaryPostfix()
        {
            var eqp    = new EquationParser().WithPostfixOperator("*", x => x * 5);
            var eq     = eqp.Parse("7*");
            var result = eq.Evaluate();

            result.Should().Be(35);
        }

        [Theory]
        [InlineData("+", 100, "+", 100, true)]
        [InlineData("+", 200, "+", 100, true)]
        [InlineData("+", 100, "+", 200, true)]
        [InlineData("+", 100, "+", 100, false)]
        [InlineData("+", 200, "+", 100, false)]
        [InlineData("+", 100, "+", 200, false)]
        [InlineData("*", 100, "+", 100, true)]
        [InlineData("*", 200, "+", 100, true)]
        [InlineData("*", 100, "+", 200, true)]
        [InlineData("*", 100, "+", 100, false)]
        [InlineData("*", 200, "+", 100, false)]
        [InlineData("*", 100, "+", 200, false)]
        public void UnaryPrefixAndPostfix(string  prefixSymbol,
                                          decimal prefixPrecedence,
                                          string  postfixSymbol,
                                          decimal postfixPrecedence,
                                          bool    leftAssociative)
        {
            var eqp = new EquationParser()
                     .WithPrefixOperator(prefixSymbol, x => x * 5)
                        .WithPrecedence(prefixPrecedence).LeftAssociative(leftAssociative)
                     .WithPostfixOperator(postfixSymbol, x => x + 7)
                        .WithPrecedence(postfixPrecedence).LeftAssociative(leftAssociative);

            var eq     = eqp.Parse($"{prefixSymbol} 9 {postfixSymbol}");
            var result = eq.Evaluate();

            if((prefixPrecedence > postfixPrecedence) || (leftAssociative && prefixPrecedence == postfixPrecedence))
                result.Should().Be((5 * 9) + 7);
            else
                result.Should().Be(5 * (9 + 7));
        }

        [Fact]
        public void UnaryPostfixChained()
        {
            var eqp    = new EquationParser().WithPostfixOperator("+", x => x + 7);
            var eq     = eqp.Parse("5++");
            var result = eq.Evaluate();

            result.Should().Be(19);
        }

        [Theory]
        [InlineData(100, 100)]
        [InlineData(100, 200)]
        [InlineData(200, 100)]
        public void UnaryPostfixChainedOrSingle(decimal shorterPostfixPrecedence, decimal longerPostfixPrecedence)
        {
            var eqp = new EquationParser()
                     .WithPostfixOperator("+",  x => x + 7).WithPrecedence(shorterPostfixPrecedence)
                     .WithPostfixOperator("++", x => x * 9).WithPrecedence(longerPostfixPrecedence);
            
            var eq     = eqp.Parse("5++");
            var result = eq.Evaluate();

            if(longerPostfixPrecedence <= shorterPostfixPrecedence)
                result.Should().Be(45); // 5++ = 5 * 9
            else
                result.Should().Be(19); // (5+)+ = (5 + 7) + 7
        }

        [Theory]
        [InlineData(100, 100, 100, true)]
        [InlineData(200, 100, 100, true)]
        [InlineData(100, 200, 100, true)]
        [InlineData(100, 100, 200, true)]
        [InlineData(200, 100, 200, true)]
        [InlineData(100, 200, 200, true)]
        [InlineData(200, 200, 100, true)]
        [InlineData(100, 200, 300, true)]
        [InlineData(100, 300, 200, true)]
        [InlineData(200, 100, 300, true)]
        [InlineData(200, 300, 100, true)]
        [InlineData(300, 200, 100, true)]
        [InlineData(300, 100, 200, true)]
        [InlineData(100, 100, 100, false)]
        [InlineData(200, 100, 100, false)]
        [InlineData(100, 200, 100, false)]
        [InlineData(100, 100, 200, false)]
        [InlineData(200, 100, 200, false)]
        [InlineData(100, 200, 200, false)]
        [InlineData(200, 200, 100, false)]
        [InlineData(100, 200, 300, false)]
        [InlineData(100, 300, 200, false)]
        [InlineData(200, 100, 300, false)]
        [InlineData(200, 300, 100, false)]
        [InlineData(300, 200, 100, false)]
        [InlineData(300, 100, 200, false)]
        public void UnaryPrefixAndChainedPostfix(decimal plusPrecedence,
                                                 decimal starPrecedence,
                                                 decimal hashPrecedence,
                                                 bool    leftAssociative)
        {
            var eqp = new EquationParser()
                     .WithPrefixOperator ("+", x => x * 7) .WithPrecedence(plusPrecedence).LeftAssociative(leftAssociative)
                     .WithPostfixOperator("*", x => x + 3) .WithPrecedence(starPrecedence).LeftAssociative(leftAssociative)
                     .WithPostfixOperator("#", x => x % 11).WithPrecedence(hashPrecedence).LeftAssociative(leftAssociative);

            var eq     = eqp.Parse("+5*#");
            var result = eq.Evaluate();
            
            // +5*# = (+5*)# = ((+5)*)# = ((7 * 5) + 3) % 11
            
            if((hashPrecedence < plusPrecedence) || (leftAssociative && hashPrecedence == plusPrecedence))
            {
                if((starPrecedence < plusPrecedence) || (leftAssociative && starPrecedence == plusPrecedence))
                    result.Should().Be(((7 * 5) + 3) % 11);
                else
                    result.Should().Be((7 * (5 + 3)) % 11);
            }
            else
                result.Should().Be(7 * ((5 + 3) % 11));
        }

        [Fact]
        public void Binary()
        {
            var eqp    = new EquationParser().WithBinaryOperator("*", (a, b) => a * b);
            var eq     = eqp.Parse("5 * 7");
            var result = eq.Evaluate();

            result.Should().Be(35);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void BinaryChained_Same(bool leftAssociative)
        {
            var eqp = new EquationParser()
                     .WithBinaryOperator("*", (a, b) => double.Parse($"{a*3}{b}")).LeftAssociative(leftAssociative);
            
            var eq     = eqp.Parse("9 * 7 * 11");
            var result = eq.Evaluate();

            if(leftAssociative)
                result.Should().Be(83111); // (9 * 7) * 11
            else
                result.Should().Be(272111); // 9 * (7 * 11)
        }

        [Theory]
        [InlineData(100, 100, true)]
        [InlineData(100, 200, true)]
        [InlineData(200, 100, true)]
        [InlineData(100, 100, false)]
        [InlineData(100, 200, false)]
        [InlineData(200, 100, false)]
        public void BinaryChained_Different(decimal plusPrecedence, decimal starPrecedence, bool leftAssociative)
        {
            var eqp = new EquationParser()
                     .WithBinaryOperator("+", (a, b) => a + b)
                         .WithPrecedence(plusPrecedence)
                         .LeftAssociative(leftAssociative)
                     .WithBinaryOperator("*", (a, b) => a * b)
                         .WithPrecedence(starPrecedence)
                         .LeftAssociative(leftAssociative);

            var eq     = eqp.Parse("9 + 7 * 4");
            var result = eq.Evaluate();

            if((plusPrecedence > starPrecedence) || (leftAssociative && plusPrecedence == starPrecedence))
                result.Should().Be((9 + 7) * 4);
            else
                result.Should().Be(9 + (7 * 4));
        }

        [Theory]
        [InlineData(100, 100)]
        [InlineData(200, 100)]
        [InlineData(100, 200)]
        public void BinaryWithOuterPrefix(decimal unaryPrecedence, decimal binaryPrecedence)
        {
            var eqp = new EquationParser()
                     .WithPrefixOperator("*", x => x * 5).WithPrecedence(unaryPrecedence)
                     .WithBinaryOperator("+", (a, b) => a + b).WithPrecedence(binaryPrecedence);

            var eq     = eqp.Parse("*7 + 9");
            var result = eq.Evaluate();

            if(unaryPrecedence >= binaryPrecedence)
                result.Should().Be((5 * 7) + 9);
            else
                result.Should().Be(5 * (7 + 9));
        }

        [Theory]
        [InlineData(100, 100)]
        [InlineData(200, 100)]
        [InlineData(100, 200)]
        public void BinaryWithOuterPostfix(decimal unaryPrecedence, decimal binaryPrecedence)
        {
            var eqp = new EquationParser()
                     .WithPostfixOperator("*", x => x * 5).WithPrecedence(unaryPrecedence)
                     .WithBinaryOperator ("+", (a, b) => a + b).WithPrecedence(binaryPrecedence);

            var eq     = eqp.Parse("7 + 9*");
            var result = eq.Evaluate();

            if(unaryPrecedence >= binaryPrecedence)
                result.Should().Be(7 + (9 * 5));
            else
                result.Should().Be((7 + 9) * 5);
        }

        [Theory]
        [InlineData(100, 100)]
        [InlineData(200, 100)]
        [InlineData(100, 200)]
        public void BinaryWithInnerPostfix(decimal unaryPrecedence, decimal binaryPrecedence)
        {
            var eqp = new EquationParser()
                     .WithPostfixOperator("*", x => x * 5).WithPrecedence(unaryPrecedence)
                     .WithBinaryOperator ("+", (a, b) => a + b).WithPrecedence(binaryPrecedence);

            var eq     = eqp.Parse("7* + 9");
            var result = eq.Evaluate();

            result.Should().Be((7 * 5) + 9);
        }

        [Theory]
        [InlineData(100, 100)]
        [InlineData(200, 100)]
        [InlineData(100, 200)]
        public void BinaryWithInnerPrefix(decimal unaryPrecedence, decimal binaryPrecedence)
        {
            var eqp = new EquationParser()
                     .WithPrefixOperator("*", x => x * 5).WithPrecedence(unaryPrecedence)
                     .WithBinaryOperator("+", (a, b) => a + b).WithPrecedence(binaryPrecedence);

            var eq     = eqp.Parse("7 + *9");
            var result = eq.Evaluate();

            result.Should().Be(7 + (5 * 9));
        }

        [Theory]
        [InlineData(100, 100, 100, 100, 100, true)]
        
        [InlineData(100, 100, 200, 100, 100, true)]
        [InlineData(100, 100, 200, 200, 100, true)]
        [InlineData(100, 100, 200, 100, 200, true)]
        [InlineData(100, 100, 100, 200, 100, true)]
        [InlineData(100, 100, 100, 200, 200, true)]
        [InlineData(100, 100, 100, 100, 200, true)]
        
        [InlineData(100, 100, 50, 100,  100, true)]
        [InlineData(100, 100, 50,  50,  100, true)]
        [InlineData(100, 100, 50,  100, 50,  true)]
        [InlineData(100, 100, 100, 50,  100, true)]
        [InlineData(100, 100, 100, 50,  50,  true)]
        [InlineData(100, 100, 100, 100, 50,  true)]
        
        [InlineData(100, 100, 50,  100, 200, true)]
        [InlineData(100, 100, 50,  200, 100, true)]
        [InlineData(100, 100, 100, 50,  200, true)]
        [InlineData(100, 100, 100, 200, 50,  true)]
        [InlineData(100, 100, 200, 50,  100, true)]
        [InlineData(100, 100, 200, 100, 50,  true)]
        public void BinaryOrUnary(decimal plusPostfixPrecedence,
                                  decimal starPrefixPrecedence,
                                  decimal plusBinaryPrecedence,
                                  decimal starBinaryPrecedence,
                                  decimal plusStarBinaryPrecedence,
                                  bool    leftAssociative)
        {
            var eqp = new EquationParser()
                     .WithPostfixOperator("+", x => x + 7)
                         .WithPrecedence(plusPostfixPrecedence)
                         .LeftAssociative(leftAssociative)
                     .WithPrefixOperator("*", x => x * 3)
                         .WithPrecedence(starPrefixPrecedence)
                         .LeftAssociative(leftAssociative)
                     .WithBinaryOperator("+", (a, b) => double.Parse($"{a}{b * 2}"))
                         .WithPrecedence(plusBinaryPrecedence)
                         .LeftAssociative(leftAssociative)
                     .WithBinaryOperator("*", (a, b) => double.Parse($"{a * 2}{b}"))
                         .WithPrecedence(starBinaryPrecedence)
                         .LeftAssociative(leftAssociative)
                     .WithBinaryOperator("+*", (a, b) => double.Parse($"{a * 2}{b * 5}"))
                         .WithPrecedence(plusStarBinaryPrecedence)
                         .LeftAssociative(leftAssociative);

            var eq     = eqp.Parse("9+*11");
            var result = eq.Evaluate();
            
            if((leftAssociative) 
                   ? (plusBinaryPrecedence <  starBinaryPrecedence && plusBinaryPrecedence <  plusStarBinaryPrecedence) 
                   : (plusBinaryPrecedence <= starBinaryPrecedence && plusBinaryPrecedence <= plusStarBinaryPrecedence))
            {
                // 9 + (*11)
                result.Should().Be(966);
            }
            else if((leftAssociative) 
                        ? (starBinaryPrecedence <= plusBinaryPrecedence && starBinaryPrecedence <= plusStarBinaryPrecedence) 
                        : (starBinaryPrecedence <  plusBinaryPrecedence && starBinaryPrecedence <  plusStarBinaryPrecedence))
            {
                // (9+) * 11
                result.Should().Be(3211);
            }
            else
            {
                // 9 +* 11
                result.Should().Be(1855);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void BinaryAndUnaryPrefixAndPostfixOnLeftOperand_differentSymbols(bool leftAssociative)
        {
            var eqp = new EquationParser()
                     .WithPrefixOperator("*", x => -x)          .LeftAssociative(leftAssociative)
                     .WithPostfixOperator("$", x => x + 10)     .LeftAssociative(leftAssociative)
                     .WithBinaryOperator("#", (a, b) => (a * b)).LeftAssociative(leftAssociative);

            var eq     = eqp.Parse("*7$#3");
            var result = eq.Evaluate();

            if(leftAssociative)
                result.Should().Be(((-7) + 10) * 3);
            else
            {
                result.Should().Be((-(7 + 10)) * 3);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void BinaryAndUnaryPrefixAndPostfixOnLeftOperand_sameSymbol(bool leftAssociative)
        {
            var eqp = new EquationParser()
                     .WithPrefixOperator("*", x => -x)          .LeftAssociative(leftAssociative)
                     .WithPostfixOperator("*", x => x + 10)     .LeftAssociative(leftAssociative)
                     .WithBinaryOperator("*", (a, b) => (a * b)).LeftAssociative(leftAssociative);

            var eq     = eqp.Parse("*7**3");
            var result = eq.Evaluate();

            if(leftAssociative)
                result.Should().Be(((-7) + 10) * 3);
            else
                result.Should().Be(-7 * -3);
        }

        [Fact]
        public void Ternary()
        {
            var eqp = new EquationParser()
               .WithInfixOperator(new List<string>() { "*", "+" }, x => x[0] * x[1] + x[2]);

            var eq     = eqp.Parse("5 * 7 + 9");
            var result = eq.Evaluate();

            result.Should().Be(5 * 7 + 9);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TernaryChained(bool leftAssociative)
        {
            var eqp = new EquationParser()
                     .WithInfixOperator(new List<string>() { "*", "+" }, 
                                        x => double.Parse($"{x[0] * 2}{x[1]}{x[2] * 3}"))
                        .LeftAssociative(leftAssociative);

            var eq     = eqp.Parse("1 * 2 + 3 * 4 + 5");
            var result = eq.Evaluate(); 
            
            if(leftAssociative)
                result.Should().Be(458415); // (1 * 2 + 3) * 4 + 5
            else
                result.Should().Be(2219245); // 1 * 2 + (3 * 4 + 5)
        }

        [Fact]
        public void TernaryNested()
        {
            var eqp = new EquationParser()
               .WithInfixOperator(new List<string>() { "*", "+" }, x => double.Parse($"{x[0]*2}{x[1]}{x[2]*3}"));

            var eq     = eqp.Parse("1 * 2 * 3 + 4 + 5");
            var result = eq.Evaluate();

            // 1 * (2 * 3 + 4) + 5
            result.Should().Be(2431215);
        }
    }
}
