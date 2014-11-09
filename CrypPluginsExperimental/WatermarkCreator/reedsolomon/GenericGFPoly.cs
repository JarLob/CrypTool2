﻿using System;
using System.Text;

 /* Original Project can be found at https://code.google.com/p/dct-watermark/
 * Ported to C# to be used within CrypTool 2 by Nils Rehwald
 * Thanks to cgaffa, ZXing and everyone else who worked on the original Project for making the original Java sources available publicly
 * Thanks to Nils Kopal for Support and Bugfixing 
 * 
 * Copyright 2007 ZXing authors Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0 Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
 * either express or implied. See the License for the specific language governing permissions and limitations under the
 * License.
 */

namespace com.google.zxing.common.reedsolomon
{

	/// <summary>
	/// <para>
	/// Represents a polynomial whose coefficients are elements of a GF. Instances of this class are immutable.
	/// </para>
	/// <para>
	/// Much credit is due to William Rucklidge since portions of this code are an indirect port of his C++ Reed-Solomon
	/// implementation.
	/// </para>
	/// 
	/// @author Sean Owen
    /// @author Ported to C# by Nils Rehwald
	/// </summary>
	internal sealed class GenericGFPoly
	{

		private readonly GenericGF field;

		private readonly int[] coefficients;

		/// <param name="field"> the <seealso cref="GenericGF"/> instance representing the field to use to perform computations </param>
		/// <param name="coefficients"> coefficients as ints representing elements of GF(size), arranged from most significant
		///            (highest-power term) coefficient to least significant </param>
		/// <exception cref="IllegalArgumentException"> if argument is null or empty, or if leading coefficient is 0 and this is not a
		///             constant polynomial (that is, it is not the monomial "0") </exception>
		internal GenericGFPoly(GenericGF field, int[] coefficients)
		{
			if (coefficients == null || coefficients.Length == 0)
			{
				throw new System.ArgumentException();
			}
			this.field = field;
			int coefficientsLength = coefficients.Length;
			if (coefficientsLength > 1 && coefficients[0] == 0)
			{
				// Leading term must be non-zero for anything except the constant polynomial "0"
				int firstNonZero = 1;
				while (firstNonZero < coefficientsLength && coefficients[firstNonZero] == 0)
				{
					firstNonZero++;
				}
				if (firstNonZero == coefficientsLength)
				{
					this.coefficients = field.Zero.coefficients;
				}
				else
				{
					this.coefficients = new int[coefficientsLength - firstNonZero];
					Array.Copy(coefficients, firstNonZero, this.coefficients, 0, this.coefficients.Length);
				}
			}
			else
			{
				this.coefficients = coefficients;
			}
		}

		internal GenericGFPoly addOrSubtract(GenericGFPoly other)
		{
			if (!this.field.Equals(other.field))
			{
				throw new System.ArgumentException("GenericGFPolys do not have same GenericGF field");
			}
			if (Zero)
			{
				return other;
			}
			if (other.Zero)
			{
				return this;
			}

			int[] smallerCoefficients = this.coefficients;
			int[] largerCoefficients = other.coefficients;
			if (smallerCoefficients.Length > largerCoefficients.Length)
			{
				int[] temp = smallerCoefficients;
				smallerCoefficients = largerCoefficients;
				largerCoefficients = temp;
			}
			int[] sumDiff = new int[largerCoefficients.Length];
			int lengthDiff = largerCoefficients.Length - smallerCoefficients.Length;
			// Copy high-order terms only found in higher-degree polynomial's coefficients
			Array.Copy(largerCoefficients, 0, sumDiff, 0, lengthDiff);

			for (int i = lengthDiff; i < largerCoefficients.Length; i++)
			{
				sumDiff[i] = GenericGF.addOrSubtract(smallerCoefficients[i - lengthDiff], largerCoefficients[i]);
			}

			return new GenericGFPoly(this.field, sumDiff);
		}

		internal GenericGFPoly[] divide(GenericGFPoly other)
		{
			if (!this.field.Equals(other.field))
			{
				throw new System.ArgumentException("GenericGFPolys do not have same GenericGF field");
			}
			if (other.Zero)
			{
				throw new System.ArgumentException("Divide by 0");
			}

			GenericGFPoly quotient = this.field.Zero;
			GenericGFPoly remainder = this;

			int denominatorLeadingTerm = other.getCoefficient(other.Degree);
			int inverseDenominatorLeadingTerm = this.field.inverse(denominatorLeadingTerm);

			while (remainder.Degree >= other.Degree && !remainder.Zero)
			{
				int degreeDifference = remainder.Degree - other.Degree;
				int scale = this.field.multiply(remainder.getCoefficient(remainder.Degree), inverseDenominatorLeadingTerm);
				GenericGFPoly term = other.multiplyByMonomial(degreeDifference, scale);
				GenericGFPoly iterationQuotient = this.field.buildMonomial(degreeDifference, scale);
				quotient = quotient.addOrSubtract(iterationQuotient);
				remainder = remainder.addOrSubtract(term);
			}

			return new GenericGFPoly[] {quotient, remainder};
		}

		/// <returns> evaluation of this polynomial at a given point </returns>
		internal int evaluateAt(int a)
		{
			if (a == 0)
			{
				// Just return the x^0 coefficient
				return getCoefficient(0);
			}
			int size = this.coefficients.Length;
			if (a == 1)
			{
				// Just the sum of the coefficients
				int result = 0;
				for (int i = 0; i < size; i++)
				{
					result = GenericGF.addOrSubtract(result, this.coefficients[i]);
				}
				return result;
			}
			int result2 = this.coefficients[0];
			for (int i = 1; i < size; i++)
			{
				result2 = GenericGF.addOrSubtract(this.field.multiply(a, result2), this.coefficients[i]);
			}
			return result2;
		}

		/// <returns> coefficient of x^degree term in this polynomial </returns>
		internal int getCoefficient(int degree)
		{
			return this.coefficients[this.coefficients.Length - 1 - degree];
		}

		internal int[] Coefficients
		{
			get
			{
				return this.coefficients;
			}
		}

		/// <returns> degree of this polynomial </returns>
		internal int Degree
		{
			get
			{
				return this.coefficients.Length - 1;
			}
		}

		/// <returns> true iff this polynomial is the monomial "0" </returns>
		internal bool Zero
		{
			get
			{
				return this.coefficients[0] == 0;
			}
		}

		internal GenericGFPoly multiply(GenericGFPoly other)
		{
			if (!this.field.Equals(other.field))
			{
				throw new System.ArgumentException("GenericGFPolys do not have same GenericGF field");
			}
			if (Zero || other.Zero)
			{
				return this.field.Zero;
			}
			int[] aCoefficients = this.coefficients;
			int aLength = aCoefficients.Length;
			int[] bCoefficients = other.coefficients;
			int bLength = bCoefficients.Length;
			int[] product = new int[aLength + bLength - 1];
			for (int i = 0; i < aLength; i++)
			{
				int aCoeff = aCoefficients[i];
				for (int j = 0; j < bLength; j++)
				{
					product[i + j] = GenericGF.addOrSubtract(product[i + j], this.field.multiply(aCoeff, bCoefficients[j]));
				}
			}
			return new GenericGFPoly(this.field, product);
		}

		internal GenericGFPoly multiply(int scalar)
		{
			if (scalar == 0)
			{
				return this.field.Zero;
			}
			if (scalar == 1)
			{
				return this;
			}
			int size = this.coefficients.Length;
			int[] product = new int[size];
			for (int i = 0; i < size; i++)
			{
				product[i] = this.field.multiply(this.coefficients[i], scalar);
			}
			return new GenericGFPoly(this.field, product);
		}

		internal GenericGFPoly multiplyByMonomial(int degree, int coefficient)
		{
			if (degree < 0)
			{
				throw new System.ArgumentException();
			}
			if (coefficient == 0)
			{
				return this.field.Zero;
			}
			int size = this.coefficients.Length;
			int[] product = new int[size + degree];
			for (int i = 0; i < size; i++)
			{
				product[i] = this.field.multiply(this.coefficients[i], coefficient);
			}
			return new GenericGFPoly(this.field, product);
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder(8 * Degree);
			for (int degree = Degree; degree >= 0; degree--)
			{
				int coefficient = getCoefficient(degree);
				if (coefficient != 0)
				{
					if (coefficient < 0)
					{
						result.Append(" - ");
						coefficient = -coefficient;
					}
					else
					{
						if (result.Length > 0)
						{
							result.Append(" + ");
						}
					}
					if (degree == 0 || coefficient != 1)
					{
						int alphaPower = this.field.log(coefficient);
						if (alphaPower == 0)
						{
							result.Append('1');
						}
						else if (alphaPower == 1)
						{
							result.Append('a');
						}
						else
						{
							result.Append("a^");
							result.Append(alphaPower);
						}
					}
					if (degree != 0)
					{
						if (degree == 1)
						{
							result.Append('x');
						}
						else
						{
							result.Append("x^");
							result.Append(degree);
						}
					}
				}
			}
			return result.ToString();
		}

	}

}