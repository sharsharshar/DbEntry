﻿using System;
using System.Collections.Generic;
using System.Data;
using Lephone.Data.SqlEntry;

namespace Lephone.Data.Common
{
    public class GroupbySumObjectHandler<T1, T2> : EmitObjectHandlerBase
    {
        public override object CreateInstance()
        {
            return new GroupBySumObject<T1, T2>();
        }

        protected override void LoadSimpleValuesByIndex(object o, IDataReader dr)
        {
            var obj = (GroupBySumObject<T1, T2>)o;
            obj.Column = (T1)Convert.ChangeType(dr.GetValue(0), typeof(T1));
            obj.Sum = (T2)Convert.ChangeType(dr.GetValue(1), typeof(T2));
        }

        protected override void LoadSimpleValuesByName(object o, IDataReader dr)
        {
        }

        protected override void LoadRelationValuesByIndex(object o, IDataReader dr)
        {
        }

        protected override void LoadRelationValuesByName(object o, IDataReader dr)
        {
        }

        protected override void GetKeyValuesDirect(Dictionary<string, object> dic, object o)
        {
        }

        protected override void SetValuesForSelectDirect(List<KeyValuePair<string, string>> keys)
        {
        }

        protected override void SetValuesForInsertDirect(KeyValueCollection values, object o)
        {
        }

        protected override void SetValuesForUpdateDirect(KeyValueCollection values, object o)
        {
        }
    }
}
