﻿using System;
using DataCloner.DataAccess;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;
using DataCloner.DataClasse.Configuration;
using NSubstitute;
using Xunit;

namespace DataCloner.Tests
{
	public class ClonerTests
	{
		public class BasicClonerTests
		{
			private readonly Cache _cache;
			private readonly Cloner _cloner;
			private readonly IQueryHelper _queryHelper;
			private readonly IQueryDispatcher _queryDispatcher;

			public BasicClonerTests()
			{
				_cache = FakeBasicDatabase.CreateDatabaseSchema();
				_queryHelper = FakeBasicDatabase.CreateData();
				_queryDispatcher = FakeBasicDatabase.CreateServer(_queryHelper);

				_cloner = new Cloner(_queryDispatcher, 
					(IQueryDispatcher dispatcher, Application app, int id, int? configId, ref Cache cache) => cache = _cache);
				_cloner.Setup(null, 0, null);
			}

			#region QueryDispatcher

			[Fact]
			public void QueryDispatcher_Select_CalledWithRowIdentifierReturnData()
			{
				var row = Make.Ri0("color", new ColumnsWithValue { { "id", 1 } });
				var result = _queryDispatcher.Select(row);

				_queryDispatcher.Received().GetQueryHelper(row);
				_queryHelper.Received().Select(row);

				Assert.Equal(Make.Obj(1), result);
			}

			[Fact]
			public void QueryDispatcher_GetQueryHelper_CalledWithRowIdentifierReturnData()
			{
				var row = Make.Ri0("color", new ColumnsWithValue { { "id", 1 } });
				var result = _queryDispatcher.GetQueryHelper(row).Select(row);

				_queryDispatcher.Received().GetQueryHelper(row);
				_queryHelper.Received().Select(row);

				Assert.Equal(Make.Obj(1), result);
			}

			[Fact]
			public void QueryDispatcher_GetQueryHelper_CalledWithIntegerReturnData()
			{
				var row = Make.Ri0("color", new ColumnsWithValue { { "id", 1 } });
				var result = _queryDispatcher.GetQueryHelper(0).Select(row);

				_queryDispatcher.Received().GetQueryHelper(row.ServerId);
				_queryHelper.Received().Select(row);

				Assert.Equal(Make.Obj(1), result);
			}

			#endregion

			#region Cloner

			[Fact]
			public void Cloner_Clone_ShouldReturnNoRow()
			{
				var row = Make.Ri0("color", new ColumnsWithValue { { "id", -1 } });
				var clones = _cloner.Clone(row, true);

				Assert.Equal(0, clones.Count);
			}

			[Fact]
			public void Cloner_Clone_Param_ShouldThrow()
			{
				Assert.Throws(typeof(ArgumentNullException), () => _cloner.Clone(null, true));
			}

			[Fact]
			public void Cloner_Clone_OneRowOneTable()
			{
				var row = Make.Ri0("color", new ColumnsWithValue { { "id", 1 } });
				var clones = _cloner.Clone(row, true);

				Assert.Equal(1, clones.Count);
				Assert.Equal("color", clones[0].Table);
				Assert.IsType<SqlVariable>(clones[0].Columns["id"]);
			}

			[Fact]
			public void Cloner_Clone_RecursiveDontCrash()
			{
				var row = Make.Ri0("person", new ColumnsWithValue { { "id", 1 } });
				var clones = _cloner.Clone(row, true);

				Assert.Equal(1, clones.Count);
				Assert.Equal("person", clones[0].Table);
				Assert.IsType<SqlVariable>(clones[0].Columns["id"]);
			}

			#endregion
		}
	}
}
