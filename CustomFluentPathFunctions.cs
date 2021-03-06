﻿extern alias dstu2;
extern alias stu3;

using Hl7.ElementModel;
// using Hl7.Fhir.FluentPath;
//using Hl7.Fhir.Model;
using Hl7.FluentPath;
using Hl7.FluentPath.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FhirPathTester
{
    public static class CustomExtensions
    {
        public static IEnumerable<dstu2.Hl7.Fhir.Model.Base> ToFhirValues(this IEnumerable<IValueProvider> results)
        {
            return results.Select(r =>
            {
                if (r == null)
                    return null;

                if (r is dstu2.Hl7.Fhir.FluentPath.PocoNavigator && (r as dstu2.Hl7.Fhir.FluentPath.PocoNavigator).FhirValue != null)
                {
                    return ((dstu2.Hl7.Fhir.FluentPath.PocoNavigator)r).FhirValue;
                }
                object result;
                if (r.Value is Hl7.FluentPath.ConstantValue)
                {
                    result = (r.Value as Hl7.FluentPath.ConstantValue).Value;
                }
                else
                {
                    result = r.Value;
                }

                if (result is bool)
                {
                    return new dstu2.Hl7.Fhir.Model.FhirBoolean((bool)result);
                }
                if (result is long)
                {
                    return new dstu2.Hl7.Fhir.Model.Integer((int)(long)result);
                }
                if (result is decimal)
                {
                    return new dstu2.Hl7.Fhir.Model.FhirDecimal((decimal)result);
                }
                if (result is string)
                {
                    return new dstu2.Hl7.Fhir.Model.FhirString((string)result);
                }
                if (result is PartialDateTime)
                {
                    var dt = (PartialDateTime)result;
                    return new dstu2.Hl7.Fhir.Model.FhirDateTime(dt.ToUniversalTime());
                }
                else
                {
                    // This will throw an exception if the type isn't one of the FHIR types!
                    return (dstu2.Hl7.Fhir.Model.Base)result;
                }
            });
        }
    }
    public class CustomFluentPathFunctions
    {
        static private SymbolTable _st;
        static public SymbolTable Scope
        {
            get
            {
                if (_st == null)
                {
                    _st = new SymbolTable().AddStandardFP();
                    // _st.Add("rand", (object f) => { return "slim"; });

                    // Custom function that returns the name of the property, rather than its value
                    _st.Add("propname", (object f) =>
                    {
                        if (f is IEnumerable<IValueProvider>)
                        {
                            object[] bits = (f as IEnumerable<IValueProvider>).Select(i =>
                            {
                                if (i is stu3.Hl7.Fhir.FluentPath.PocoNavigator)
                                {
                                    return (i as stu3.Hl7.Fhir.FluentPath.PocoNavigator).Name;
                                }
                                if (i is dstu2.Hl7.Fhir.FluentPath.PocoNavigator)
                                {
                                    return (i as dstu2.Hl7.Fhir.FluentPath.PocoNavigator).Name;
                                }
                                return "?";
                            }).ToArray();
                            return FhirValueList.Create(bits);
                        }
                        return FhirValueList.Create(new object[] { "?" } );
                    });
                    _st.Add("pathname", (object f) =>
                    {
                        if (f is IEnumerable<IValueProvider>)
                        {
                            object[] bits = (f as IEnumerable<IValueProvider>).Select(i =>
                            {
                                if (i is stu3.Hl7.Fhir.FluentPath.PocoNavigator)
                                {
                                    return (i as stu3.Hl7.Fhir.FluentPath.PocoNavigator).Path;
                                }
                                if (i is dstu2.Hl7.Fhir.FluentPath.PocoNavigator)
                                {
                                    return (i as dstu2.Hl7.Fhir.FluentPath.PocoNavigator).Path;
                                }
                                return "?";
                            }).ToArray();
                            return FhirValueList.Create(bits);
                        }
                        return FhirValueList.Create(new object[] { "?" });
                    });
                    //_st.Add("commonpathname", (object f) =>
                    //{
                    //    if (f is IEnumerable<IValueProvider>)
                    //    {
                    //        object[] bits = (f as IEnumerable<IValueProvider>).Select(i =>
                    //        {
                    //            if (i is PocoNavigator)
                    //            {
                    //                return (i as PocoNavigator).CommonPath;
                    //            }
                    //            return "?";
                    //        }).ToArray();
                    //        return FhirValueList.Create(bits);
                    //    }
                    //    return FhirValueList.Create(new object[] { "?" });
                    //});

                    // Custom function for evaluating the date operation (custom Healthconnex)
                    _st.Add("dateadd", (PartialDateTime f, string field, long amount) =>
                    {
                        DateTimeOffset dto = f.ToUniversalTime();
                        int value = (int)amount;

                        // Need to convert the amount and field to compensate for partials
                        //TimeSpan ts = new TimeSpan();

                        switch (field)
                        {
                            case "yy": dto = dto.AddYears(value); break;
                            case "mm": dto = dto.AddMonths(value); break;
                            case "dd": dto = dto.AddDays(value); break;
                            case "hh": dto = dto.AddHours(value); break;
                            case "mi": dto = dto.AddMinutes(value); break;
                            case "ss": dto = dto.AddSeconds(value); break;
                        }
                        PartialDateTime changedDate = PartialDateTime.Parse(PartialDateTime.FromDateTime(dto).ToString().Substring(0, f.ToString().Length));
                        return changedDate;
                    });
                }
                return _st;
            }
        }

    }
}
