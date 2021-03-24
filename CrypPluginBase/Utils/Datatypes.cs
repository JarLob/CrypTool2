﻿/*
   Copyright 2019 Simon Leischnig, based on the work of Soeren Rinne

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypTool.PluginBase.Utils
{
    namespace Datatypes
    {
        public class Option<T>
        {
            internal readonly T value;
            public bool IsNone { get; }
            public bool IsSome { get; }
            internal Option(T value, bool isNone)
            {
                this.value = value;
                this.IsNone = isNone;
                this.IsSome = ! isNone;
            }
            public M Match<M>(Func<T, M> Some, Func<M> None)
            {
                if (! this.IsNone)
                {
                    return Some(this.value);
                }
                else
                {
                    return None();
                }
            }
            public void Match(Action<T> Some, Action None)
            {
                if (! this.IsNone)
                {
                    Some(this.value);
                }
                else
                {
                    None();
                }
            }
            public T IfNone(Func<T> None)
            {
                return Match(value => value, None);
            }
            public T IfNone(T None)
            {
                return Match(value => value, () => None);
            }

            public Option<T2> Map<T2>(Func<T,T2> f)
            {
                if (IsSome)
                {
                    return Option<T2>.Some(f.Invoke(this.value));
                }
                return Option<T2>.None();
            }
            internal static T checkNull(T value)
            {

                if (value == null)
                {
                    throw new Exception("Option cannot be Some(null)");
                }
                return value;
            }

            // --- static API
            public static Option<T> Some(T value)
            {
                return new Option<T>(checkNull(value), false);
            }
            public static Option<T> None()
            {
                return new Option<T>(default(T), true);
            }

            public T Get()
            {
                if (this.IsNone)
                {
                    throw new Exception("Get() invoked on None");
                }
                return this.value;
            }
            public string ToStringWithR(Func<T, String> rts)
            {
                return this.Match(some => $"Some({rts.Invoke(some)})", () => "None");
            }
            public override string ToString()
            {
                return this.ToStringWithR((some) => some.ToString());
            }
        }
        public class Either<L, R>
        {
            internal readonly Option<R> right;
            internal readonly Option<L> left;
            public Boolean IsLeft { get { return left.IsSome; } }
            public Boolean IsRight { get { return right.IsSome; } }

            public string ToStringRecursive(Func<dynamic, String> toString)
            {
                return this.Match(right => $"Right({toString.Invoke(right)})", left => left.ToString());
            }
            public override string ToString()
            {
                return ToStringRecursive((r) => r.ToString());
            }

            private Either(Option<L> left, Option<R> right)
            {
                if (!(left.IsSome ^ right.IsSome))
                {
                    throw new Exception("Either cannot be both Left and Right");
                }
                this.left = left;
                this.right = right;
            }
            public void Match(Action<R> R, Action<L> L)
            {
                if (IsRight)
                {
                    R.Invoke(this.right.Get());
                    return;
                }
                L.Invoke(this.left.Get());
            }
            public Ret Match<Ret>(Func<R, Ret> R, Func<L, Ret> L) => this.right.Map(R).IfNone(() => L.Invoke(this.left.Get()));

            public static Either<L, R> Left(L left) => new Either<L, R>(Option<L>.Some(left), Option<R>.None());
            public static Either<L, R> Right(R right) => new Either<L, R>(Option<L>.None(), Option<R>.Some(right));
            public Either<L, R2> Map<R2>(Func<R, R2> f) => new Either<L, R2>(left, right.Map(f));
            public Either<L2, R> MapLeft<L2>(Func<L, L2> f) => new Either<L2, R>(left.Map(f), right);

            public Option<R> ToOption() => this.right;

            public R IfLeft(R rightVal) => this.right.IfNone(rightVal);
            public L IfRight(L leftVal) => this.left.IfNone(leftVal);
            // This method is just so it looks nicer
            public R OrIfError(R rightVal) => this.IfLeft(rightVal);

            public L GetLeft()
            {
                if (IsRight)
                {
                    throw new Exception("GetLeft called on Right Either");
                }
                return this.left.Get();
            }
            public R GetRight()
            {
                if (IsLeft)
                {
                    throw new Exception("GetLeft called on Right Either");
                }
                return this.right.Get();
            }
        }

        // Wrapped values with event infrastructure
        public interface IStoredValue<T>
        {
            T Value { get; set; }
        }
        public class Box<T> : IStoredValue<T>
        {
            public Action<T> OnChange { get; set; } = (val) => { };
            public Action<T,T> OnChangeFromTo { get; set; } = (old,newVal) => { };
            private T _value;

            public T Value { get { return this._value; } set { 
                    this.SetValue(value); } 
            }
            public Box(T initial) { this._value = initial; }
            private void SetValue(T newVal) { var oldVal = this.Value; this._value = newVal; this.OnChange(newVal); this.OnChangeFromTo(oldVal, newVal); }
        }
        public class HistoryBox<T> : IStoredValue<T>
        {
            public Action<T> OnChange { get; set; } = (val) => { };
            public List<T> History = new List<T>();

            public void Record(T val) => 
                this.Value = val;
            public T Value { get { if (History.Count == 0) throw new Exception("No value recorded in this history");  return History[History.Count-1]; } set { this.SetValue(value); } }
            public T Last { get => Value; }
            public HistoryBox() { this.History = new List<T>(); }
            public HistoryBox(T initial) { this.History = new List<T>(); this.History.Add(initial); }
            private void SetValue(T newVal) { this.History.Add(newVal); this.OnChange(newVal); }
        }

        public static class Datatypes
        {
            public static Option<T> OptionNullable<T>(T value) => value == null ? Option<T>.None() : Option<T>.Some(value);
            public static Option<T> Some<T>(T value) => Option<T>.Some(value);
            public static Option<T> None<T>() => Option<T>.None();
            public static Either<L, R> Left<L, R>(L left) => Either<L, R>.Left(left);
            public static Either<L, R> Right<L, R>(R right) => Either<L, R>.Right(right);

            public static List<T> Sequence<T>(params T[] list) => new List<T>(list);
            public static Option<V> GetOpt<K,V>(this Dictionary<K,V> dict, K key)
            {
                V val;
                var hadVal = dict.TryGetValue(key, out val);
                return hadVal ? Some(val) : None<V>();
            }
            public static V GetOr<K,V>(this Dictionary<K,V> dict, K key, V def)
            {
                return dict.GetOpt(key).IfNone(def);
            }

        }
    }
}
