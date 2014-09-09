﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

using DataCloner.Framework;
using DataCloner.Enum;

namespace DataCloner.DataClasse.Cache
{
    /// <summary>
    /// Server / database / schema / table / primarykey source value = primarykey destination value
    /// </summary>
    internal sealed class KeyRelationship
    {
        private Dictionary<Int16, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<object[], object[]>>>>> _dic = 
            new Dictionary<Int16, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<object[], object[]>>>>>();

        public object[] GetKey(Int16 server, string database, string schema, string table, object[] keyValuesSource)
        {
            if (_dic.ContainsKey(server) &&
                _dic[server].ContainsKey(database) &&
                _dic[server][database].ContainsKey(schema) &&
                _dic[server][database][schema].ContainsKey(table) &&
                _dic[server][database][schema][table].ContainsKey(keyValuesSource))
                return _dic[server][database][schema][table][keyValuesSource];
            return null;
        }

        public void SetKey(Int16 server, string database, string schema, string table, object[] keyValuesSource, object[] keyValuesDestination)
        {
            if (!_dic.ContainsKey(server))
                _dic.Add(server, new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<object[], object[]>>>>());

            if (!_dic[server].ContainsKey(database))
                _dic[server].Add(database, new Dictionary<string, Dictionary<string, Dictionary<object[], object[]>>>());

            if (!_dic[server][database].ContainsKey(schema))
                _dic[server][database].Add(schema, new Dictionary<string, Dictionary<object[], object[]>>());

            if (!_dic[server][database][schema].ContainsKey(table))
                _dic[server][database][schema].Add(table, new Dictionary<object[], object[]>(StructuralEqualityComparer<object[]>.Default));

            if (_dic[server][database][schema][table].ContainsKey(keyValuesSource))
                throw new ArgumentException("Key already exist!");
            else
                _dic[server][database][schema][table][keyValuesSource] = keyValuesDestination;           
        }

        public object[] this[Int16 server, string database, string schema, string table, object[] keyValuesSource, object[] keyValuesDestination]
        {
            get
            {
                if (_dic.ContainsKey(server) &&
                    _dic[server].ContainsKey(database) &&
                    _dic[server][database].ContainsKey(schema) &&
                    _dic[server][database][schema].ContainsKey(table) &&
                    _dic[server][database][schema][table].ContainsKey(keyValuesSource))
                    return _dic[server][database][schema][table][keyValuesSource];
                return null;
            }
            set
            {
                if (!_dic.ContainsKey(server))
                    _dic.Add(server, new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<object[], object[]>>>>());

                if (!_dic[server].ContainsKey(database))
                    _dic[server].Add(database, new Dictionary<string, Dictionary<string, Dictionary<object[], object[]>>>());

                if (!_dic[server][database].ContainsKey(schema))
                    _dic[server][database].Add(schema, new Dictionary<string, Dictionary<object[], object[]>>());

                if (!_dic[server][database][schema].ContainsKey(table))
                    _dic[server][database][schema].Add(table, new Dictionary<object[], object[]>(StructuralEqualityComparer<object[]>.Default));

                if (_dic[server][database][schema][table].ContainsKey(keyValuesSource))
                    throw new ArgumentException("Key already exist!");
                else
                    _dic[server][database][schema][table][keyValuesSource] = keyValuesDestination;
            }
        }      
    }    
}