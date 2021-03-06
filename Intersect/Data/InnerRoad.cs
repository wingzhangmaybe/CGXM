﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using System.Data.SqlClient;

namespace Intersect
{
    public class InnerRoad : DataBase
    {
        private const int IRNAME_MAX_LENGTH = 50;
        private const string DEFAULT_IRNAME = "内部路";

        private int irID;
        public int id
        {
            get
            {
                return irID;
            }
            set
            {
                irID = value;
                onPropertyChanged("id");
            }
        }
        private int prID;
        public int programID
        {
            get
            {
                return prID;
            }
            set
            {
                prID = value;
                onPropertyChanged("programID");
            }
        }
        private int vID;
        public int villageID
        {
            get
            {
                return vID;
            }
            set
            {
                vID = value;
                onPropertyChanged("villageID");
            }
        }
        private string irName;
        public string name
        {
            get
            {
                return irName;
            }
            set
            {
                irName = value;
                onPropertyChanged("name");
            }
        }
        private string irPath;
        public string path
        {
            get
            {
                return irPath;
            }
            set
            {
                irPath = value;
                onPropertyChanged("path");
            }
        }
        public ILineElement lineElement;

        public InnerRoad()
        {
            irID = C.ERROR_INT;
            prID = C.ERROR_INT;
            vID = C.ERROR_INT;
            irName = DEFAULT_IRNAME;
            irPath = C.ERROR_STRING;
        }

        private void initBySqlDataReader(SqlDataReader reader)
        {
            reader.Read();
            irID = Int32.Parse(reader[0].ToString());
            prID = Int32.Parse(reader[1].ToString());
            vID = Int32.Parse(reader[2].ToString());
            irName = reader[3].ToString();
            irPath = reader[4].ToString();
            if (irPath != "")
            {
                List<Point> pointList = InnerRoad.ConvertStringToPointList(irPath);
                lineElement = GisUtil.getILineElementFromPointList(pointList);
            }
        }

        public override string checkValid(List<string> shieldVariableList = null)
        {
            if (shieldVariableList == null)
                shieldVariableList = new List<string>();
            if (!shieldVariableList.Contains("id") && irID == C.ERROR_INT)
                return C.INNER_ERROR_TIP;
            if (!shieldVariableList.Contains("programID") && prID == C.ERROR_INT)
                return C.INNER_ERROR_TIP;
            if (!shieldVariableList.Contains("villageID") && vID == C.ERROR_INT)
                return C.INNER_ERROR_TIP;
            if (!shieldVariableList.Contains("name") && (irName.Length == 0 || irName.Length > IRNAME_MAX_LENGTH))
                return String.Format("内部路名长度须在0-{0}之间.", IRNAME_MAX_LENGTH);
            if (!shieldVariableList.Contains("path") && irPath.Length == 0)
                return "内部路路径不能为空";
            return "";
        }

        protected override bool isValid(List<string> shieldVariableList = null)
        {
            if (shieldVariableList == null)
                shieldVariableList = new List<string>();
            if (!shieldVariableList.Contains("irID") && irID == C.ERROR_INT)
                return false;
            if (!shieldVariableList.Contains("prID") && prID == C.ERROR_INT)
                return false;
            if (!shieldVariableList.Contains("vID") && vID == C.ERROR_INT)
                return false;
            if (!shieldVariableList.Contains("irName") && (irName.Length == 0 || irName.Length > IRNAME_MAX_LENGTH))
                return false;
            if (!shieldVariableList.Contains("irPath") && irPath.Length == 0)
                return false;
            return true;
        }

        public override bool save()
        {
            if (!isValid(new List<string>() { "irID" }))
                return false;
            string sqlCommand = String.Format(@"insert into InnerRoad (prID,vID,irName,irPath) values ({0},{1},'{2}','{3}')"
                , prID, vID, irName, irPath);
            Sql sql = new Sql();
            return sql.insertInnerRoad(sqlCommand);
        }

        public bool saveWithoutCheck()
        {
            string sqlCommand = String.Format(@"insert into InnerRoad (prID,vID,irName,irPath) values ({0},{1},'{2}','{3}')"
                , prID, vID, irName, irPath);
            Sql sql = new Sql();
            return sql.insertInnerRoad(sqlCommand);
        }

        public override bool update()
        {
            if (!isValid())
                return false;
            string sqlCommand = String.Format(@"update InnerRoad set prID={0},vID={1},irName='{2}',irPath='{3}' where irID={4}"
                , prID, vID, irName, irPath, irID);
            Sql sql = new Sql();
            return sql.updateInnerRoad(sqlCommand);
        }

        public bool saveOrUpdate()
        {
            if (id == C.ERROR_INT)
            {
                return save();
            }
            else
            {
                return update();
            }
        }

        public void updatePath()
        {
            List<Point> pointList = GisUtil.getPointListFromILineElement(lineElement);
            irPath = InnerRoad.ConvertPointListToString(pointList);
        }

        public override bool delete()
        {
            if (!isValid(new List<string>() { "prID", "vID", "irName", "irPath" }))
                return false;
            string sqlCommand = String.Format(@"delete from InnerRoad where irID={0}", irID);
            Sql sql = new Sql();
            return sql.deleteInnerRoad(sqlCommand);
        }

        public override bool select()
        {
            if (!isValid(new List<string>() { "prID", "vID", "irName", "irPath" }))
                return false;
            string sqlCommand = String.Format(@"select * from InnerRoad where irID={0}", irID);
            Sql sql = new Sql();
            SqlDataReader reader = sql.selectInnerRoad(sqlCommand);
            initBySqlDataReader(reader);
            sql.closeConnection();
            return true;
        }

        public static string ConvertPointListToString(List<Point> pointList)
        {
            string pointString = "";
            foreach (Point point in pointList)
            {
                pointString += String.Format(@"{0},{1} ", point.x, point.y);
            }
            pointString = pointString.Substring(0, pointString.Length - 1);
            return pointString;
        }

        public static List<Point> ConvertStringToPointList(string pointString)
        {
            List<Point> pointList = new List<Point>();
            List<string> singlePointStringList = new List<string>(pointString.Split(' '));
            foreach (string singlePointString in singlePointStringList)
            {
                string[] singlePointArray = singlePointString.Split(',');
                Point point = new Point();
                point.x = Double.Parse(singlePointArray[0]);
                point.y = Double.Parse(singlePointArray[1]);
                pointList.Add(point);
            }
            return pointList;
        }

        public bool compare(InnerRoad innerRoad)
        {
            if (irID != innerRoad.id)
                return false;
            if (prID != innerRoad.programID)
                return false;
            if (vID != innerRoad.villageID)
                return false;
            if (irName != innerRoad.name)
                return false;
            if (irPath != innerRoad.path)
                return false;
            return true;
        }

        public static int GetLastInnerRoadID()
        {
            string sqlCommand = String.Format("select max(irID) from InnerRoad");
            Sql sql = new Sql();
            return sql.selectMaxInnerRoadID(sqlCommand);
        }
    }
}
