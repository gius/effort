﻿#region License

// Copyright (c) 2011 Effort Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#endregion

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Effort.Internal.Common;

namespace Effort.Internal.DbCommandTreeTransformation
{
    internal class LinqMethodProvider
    {
        public static readonly LinqMethodProvider Instance = new LinqMethodProvider();

        private Lazy<MethodInfo> select;
        private Lazy<MethodInfo> selectMany;
        private Lazy<MethodInfo> selectManyWithResultSelector;
        private Lazy<MethodInfo> join;
        private Lazy<MethodInfo> leftOuterJoin;

        private Lazy<MethodInfo> count;
        private Lazy<MethodInfo> where;
        private Lazy<MethodInfo> take;
        private Lazy<MethodInfo> skip;
        private Lazy<MethodInfo> groupBy;

        private Lazy<MethodInfo> orderBy;
        private Lazy<MethodInfo> orderByDescending;
        private Lazy<MethodInfo> thenBy;
        private Lazy<MethodInfo> thenByDescending;

        private Lazy<MethodInfo> firstOrDefault;
        private Lazy<MethodInfo> first;
        private Lazy<MethodInfo> any;

        private Lazy<MethodInfo> distinct;
        private Lazy<MethodInfo> except;
        private Lazy<MethodInfo> intersect;
        private Lazy<MethodInfo> union;
        private Lazy<MethodInfo> concat;

        private LinqMethodProvider()
        {
            LazyThreadSafetyMode safety = LazyThreadSafetyMode.PublicationOnly;

            this.select = new Lazy<MethodInfo>(CreateSelect, safety);
            this.selectMany = new Lazy<MethodInfo>(CreateSelectMany, safety);
            this.selectManyWithResultSelector = new Lazy<MethodInfo>(CreateSelectManyWithResultSelector, safety);
            this.join = new Lazy<MethodInfo>(CreateJoin, safety);
            this.leftOuterJoin = new Lazy<MethodInfo>(CreateLeftOuterJoin, safety);

            this.count = new Lazy<MethodInfo>(CreateCount, safety);
            this.where = new Lazy<MethodInfo>(CreateWhere, safety);
            this.take = new Lazy<MethodInfo>(CreateTake, safety);
            this.skip = new Lazy<MethodInfo>(CreateSkip, safety);
            this.groupBy = new Lazy<MethodInfo>(CreateGroupBy, safety);

            this.orderBy = new Lazy<MethodInfo>(CreateOrderBy, safety);
            this.orderByDescending = new Lazy<MethodInfo>(CreateOrderByDescending, safety);
            this.thenBy = new Lazy<MethodInfo>(CreateThenBy, safety);
            this.thenByDescending = new Lazy<MethodInfo>(CreateThenByDescending, safety);

            this.first = new Lazy<MethodInfo>(CreateFirst, safety);
            this.firstOrDefault = new Lazy<MethodInfo>(CreateFirstOrDefault, safety);
            this.any = new Lazy<MethodInfo>(CreateAny, safety);

            this.distinct = new Lazy<MethodInfo>(CreateDistinct, safety);
            this.except = new Lazy<MethodInfo>(CreateExcept, safety);
            this.intersect = new Lazy<MethodInfo>(CreateIntersect, safety);
            this.union = new Lazy<MethodInfo>(CreateUnion, safety);
            this.concat = new Lazy<MethodInfo>(CreateConcat, safety);

        }

        #region MethodInfo provider properties

        public MethodInfo Select
        {
            get { return this.select.Value; }
        }

        public MethodInfo SelectMany
        {
            get { return this.selectMany.Value; }
        }

        public MethodInfo SelectManyWithResultSelector
        {
            get { return this.selectManyWithResultSelector.Value; }
        }

        public MethodInfo Join
        {
            get { return this.join.Value; }
        }

        public MethodInfo LeftOuterJoin
        {
            get { return this.leftOuterJoin.Value; }
        }

        public MethodInfo Count
        {
            get { return this.count.Value; }
        }

        public MethodInfo Where
        {
            get { return this.where.Value; }
        }

        public MethodInfo Take
        {
            get { return this.take.Value; }
        }

        public MethodInfo Skip
        {
            get { return this.skip.Value; }
        }

        public MethodInfo GroupBy
        {
            get { return this.groupBy.Value; }
        }

        public MethodInfo OrderBy
        {
            get { return this.orderBy.Value; }
        }

        public MethodInfo OrderByDescending
        {
            get { return this.orderByDescending.Value; }
        }

        public MethodInfo ThenBy
        {
            get { return this.thenBy.Value; }
        }

        public MethodInfo ThenByDescending
        {
            get { return this.thenByDescending.Value; }
        }

        public MethodInfo First
        {
            get { return this.first.Value; }
        }

        public MethodInfo FirstOrDefault
        {
            get { return this.firstOrDefault.Value; }
        }

        public MethodInfo Any
        {
            get { return this.any.Value; }
        }

        public MethodInfo Distinct
        {
            get { return this.distinct.Value; }
        }

        public MethodInfo Except
        {
            get { return this.except.Value; }
        }

        public MethodInfo Intersect 
        {
            get { return this.intersect.Value; }
        }

        public MethodInfo Union
        {
            get { return this.union.Value; }
        }

        public MethodInfo Concat
        {
            get { return this.concat.Value; }
        }

        #endregion

        #region MethodInfo factory methods

        private MethodInfo GetMethod<T>(Expression<Func<IQueryable<object>, T>> function)
        {
            MethodInfo result = ReflectionHelper.GetMethodInfo<IQueryable<object>, T>(function);

            return result.GetGenericMethodDefinition();
        }

        private MethodInfo GetOrderedMethod<T>(Expression<Func<IOrderedQueryable<object>, T>> function)
        {
            MethodInfo result = ReflectionHelper.GetMethodInfo<IOrderedQueryable<object>, T>(function);

            return result.GetGenericMethodDefinition();
        }

        private MethodInfo CreateSelect()
        {
            return GetMethod(x => x.Select(e => e));
        }

        private MethodInfo CreateSelectMany()
        {
            return GetMethod(x => x.SelectMany(e => Enumerable.Empty<object>()));
        }

        private MethodInfo CreateSelectManyWithResultSelector()
        {
            return GetMethod(x => x.SelectMany(e => 
                Enumerable.Empty<object>(), 
                (e, r) => new object()));
        }

        private MethodInfo CreateJoin()
        {
            return GetMethod(x => x.Join(
                Enumerable.Empty<object>(), 
                l => new object(), 
                r => new object(), 
                (l, r) => new object()));
        }

        private MethodInfo CreateLeftOuterJoin()
        {
            return GetMethod(x => NMemory.Linq.QueryableEx.LeftOuterJoin(
                x, 
                Enumerable.Empty<object>(), 
                l => new object(), 
                r => new object(), 
                (l, r) => new object()));
        }

        private MethodInfo CreateCount()
        {
            // This needs Enumerable method, because of IGrouping
            return GetMethod(q => Enumerable.Count(q));
        }

        private MethodInfo CreateWhere()
        {
            return GetMethod(x => x.Where(e => true));
        }

        private MethodInfo CreateTake()
        {
            return GetMethod(x => x.Take(0));
        }

        private MethodInfo CreateSkip()
        {
            return GetMethod(x => x.Skip(0));
        }

        private MethodInfo CreateGroupBy()
        {
            return GetMethod(x => x.GroupBy(e => e));
        }

        private MethodInfo CreateOrderBy()
        { 
            return GetMethod(x => x.OrderBy(e => e));
        }

        private MethodInfo CreateOrderByDescending()
        {
            return GetMethod(x => x.OrderByDescending(e => e));
        }

        private MethodInfo CreateThenBy()
        {
            return GetOrderedMethod(x => x.ThenBy(e => e));
        }

        private MethodInfo CreateThenByDescending()
        {
            return GetOrderedMethod(x => x.ThenByDescending(e => e));
        }

        private MethodInfo CreateFirst()
        {
            return GetMethod(x => x.First());
        }

        private MethodInfo CreateFirstOrDefault()
        {
            return GetMethod(x => x.FirstOrDefault());
        }

        private MethodInfo CreateAny()
        {
            return GetMethod(x => x.Any());
        }

        private MethodInfo CreateDistinct()
        {
            return GetMethod(x => x.Distinct());
        }

        private MethodInfo CreateExcept()
        {
            return GetMethod(x => x.Except(Enumerable.Empty<object>()));
        }

        private MethodInfo CreateConcat()
        {
            return GetMethod(x => x.Concat(Enumerable.Empty<object>()));
        }

        private MethodInfo CreateUnion()
        {
            return GetMethod(x => x.Union(Enumerable.Empty<object>()));
        }

        private MethodInfo CreateIntersect()
        {
            return GetMethod(x => x.Intersect(Enumerable.Empty<object>()));
        }

        #endregion

    }
}