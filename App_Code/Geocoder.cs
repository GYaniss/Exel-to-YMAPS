using System;
using System.Net;
using System.IO;
using suggestionscsharp;

/// <summary>
/// Сводное описание для Geocode
/// </summary>
public class GeoCoder
{
    public static bool DecodeAdress (string Adress, out string Coord1, out string Coord2, out string ErrorText)
    {
        ErrorText = null; Coord1 = null; Coord2 = null;
        bool Result = DecodeAdress_DaData(Adress, out Coord1, out Coord2, out ErrorText);
        if (Result == false) { Result = DecodeAdress_Yndex(Adress, out Coord1, out Coord2, out ErrorText); }
        if (Result == false) { Result = DecodeAdress_Sputnik(Adress, out Coord1, out Coord2, out ErrorText); }
        return Result;
    }

    static bool DecodeAdress_DaData(string Adress, out string Coord1, out string Coord2, out string ErrorText)
    {
        ErrorText = null; Coord1 = null; Coord2 = null;
        bool Result = false;
        string token = "";
        string ApiUrl = "https://suggestions.dadata.ru/suggestions/api/4_1/rs";

        try
        {
            SuggestClient api = new SuggestClient(token, ApiUrl);
            SuggestAddressResponse response = api.QueryAddress(Adress);
            if (response.suggestions.Count > 0)
            {
                SuggestAddressResponse.Suggestions A0 = response.suggestions[0];
                AddressData Otvet = A0.data;
                if (Otvet.geo_lat != null && Otvet.geo_lon != null)
                {
                    Coord1 = Otvet.geo_lat;
                    Coord2 = Otvet.geo_lon;
                    Result = true;
                }
                else
                {
                    ErrorText = "Координат нет";
                }
            }
            else
            {
                ErrorText = "Ответ пуст";
            }
        }
        catch (Exception err)
        {
            ErrorText = err.Message;
        }
        return Result;
    }

    
    static bool DecodeAdress_Yndex (string Adress, out string Coord1, out string Coord2, out string ErrorText)
    {
        string Otvet = ""; ErrorText = null; Coord1 = null; Coord2 = null;
        string strUrl = "https://geocode-maps.yandex.ru/1.x/?format=json&results=1&geocode=" + Adress.Trim().Replace(" ", "%20");
        bool Result = false;

        try
        {
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(strUrl);
            myHttpWebRequest.Method = "GET";
            myHttpWebRequest.Accept = "text";
            myHttpWebRequest.Headers.Add("Accept-Language", "ru-RU");
            myHttpWebRequest.Headers.Add("Cache-Control", "private, no-cache");
            myHttpWebRequest.KeepAlive = true;
            myHttpWebRequest.AllowAutoRedirect = true;
            using (HttpWebResponse req = (HttpWebResponse)myHttpWebRequest.GetResponse())
            {
                if (req.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = req.GetResponseStream())
                    {
                        if (stream != null)
                        {
                            StreamReader sr = new StreamReader(stream, true);
                            Otvet = sr.ReadToEnd();
                            //Yndex:
                            if (Otvet.IndexOf("Point") > 0 && Otvet.IndexOf("pos") > 0)
                            {
                                Otvet = Otvet.Substring(Otvet.IndexOf("\"Point\":{\"pos\":\"") + 16);
                                Otvet = Otvet.Remove(Otvet.IndexOf('"'));
                                Coord2 = Otvet.Remove(Otvet.IndexOf(' '));
                                Coord1 = Otvet.Substring(Otvet.IndexOf(' ') + 1);
                                Result = true;
                            }
                            else
                            {
                                ErrorText = "Координат нет";
                            }
                        }
                        else
                        {
                            ErrorText = "Ответ пуст";
                        }
                        stream.Close();
                    }
                }
                else
                {
                    ErrorText = "Ошибка HTTP:" + ((int)req.StatusCode).ToString() + " " + req.StatusDescription;
                }
                req.Close();
            }
        }
        catch (WebException err)
        {
            if (err.Status == WebExceptionStatus.ProtocolError)
            {
                ErrorText = "Ошибка сети: Ошибка протокола HTTP ";
                ; using (Stream stream = err.Response.GetResponseStream())
                {
                    if (stream != null)
                    {
                        StreamReader sr = new StreamReader(stream, true);
                        ErrorText = ErrorText + sr.ReadToEnd();
                        sr.Close();
                    }
                }
            }
            if (err.Status == WebExceptionStatus.CacheEntryNotFound) { ErrorText = "Ошибка сети: Указанная запись не была найдена в кэше"; }
            if (err.Status == WebExceptionStatus.ConnectFailure) { ErrorText = "Ошибка сети: С точкой удаленной службы нельзя связаться на транспортном уровне"; }
            if (err.Status == WebExceptionStatus.ConnectionClosed) { ErrorText = "Ошибка сети: Подключение было преждевреммно закрыто"; }
            if (err.Status == WebExceptionStatus.KeepAliveFailure) { ErrorText = "Ошибка сети: Неожиданно закрыто подключение для запроса, задающего загаловок Keep-alive"; }
            if (err.Status == WebExceptionStatus.MessageLengthLimitExceeded) { ErrorText = "Ошибка сети: Принято сообщение о превышении заданного ограничения при передаче запроса или приеме ответа сервера"; }
            if (err.Status == WebExceptionStatus.NameResolutionFailure) { ErrorText = "Ошибка сети: Служба разрешения имен не можеть разрешить имя узла"; }
            if (err.Status == WebExceptionStatus.Pending) { ErrorText = "Ошибка сети: Внутренний асинхронный запрос находиться в очереди"; }
            if (err.Status == WebExceptionStatus.PipelineFailure) { ErrorText = "Ошибка сети: Запрос являлся конвеерным запросом и подключение было закрыто до получения запроса"; }
            if (err.Status == WebExceptionStatus.ProxyNameResolutionFailure) { ErrorText = "Ошибка сети: Служба разрешения имен не может распознать имя узла проски сервера"; }
            if (err.Status == WebExceptionStatus.ReceiveFailure) { ErrorText = "Ошибка сети: От удаленного сервера не был получен полный ответ"; }
            if (err.Status == WebExceptionStatus.RequestCanceled) { ErrorText = "Ошибка сети: Запрос был отменен"; }
            if (err.Status == WebExceptionStatus.RequestProhibitedByCachePolicy) { ErrorText = "Ошибка сети: Запрос не разрешен политикой кеширования"; }
            if (err.Status == WebExceptionStatus.RequestProhibitedByProxy) { ErrorText = "Ошибка сети: Этот запрос не разрешен прокси сервером"; }
            if (err.Status == WebExceptionStatus.SendFailure) { ErrorText = "Ошибка сети: Полный запрос не был передан на удаленный сервер"; }
            if (err.Status == WebExceptionStatus.ServerProtocolViolation) { ErrorText = "Ошибка сети: Ответ сервера не являлся допустимым ответом HTTP"; }
            if (err.Status == WebExceptionStatus.Timeout) { ErrorText = "Ошибка сети: В течение времени времени ожидания ответ получен не был"; }
            if (err.Status == WebExceptionStatus.TrustFailure) { ErrorText = "Ошибка сети: Сертификат сервера не может быть проверен"; }
            if (err.Status == WebExceptionStatus.UnknownError) { ErrorText = "Ошибка сети: Возникло исключение не известного типа"; }
        }
        catch (Exception err)
        {
            ErrorText = err.Message;
        }
        return Result;
    }

    static bool DecodeAdress_Sputnik (string Adress, out string Coord1, out string Coord2, out string ErrorText)
    {
        string Otvet = ""; ErrorText = null; Coord1 = null; Coord2 = null;
        string strUrl = "http://search.maps.sputnik.ru/search/addr?addr_limit=1&q=" + Adress.Trim().Replace(" ", "%20");
        bool Result = false;

        try
        {
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(strUrl);
            myHttpWebRequest.Method = "GET";
            myHttpWebRequest.Accept = "text";
            myHttpWebRequest.Headers.Add("Accept-Language", "ru-RU");
            myHttpWebRequest.Headers.Add("Cache-Control", "private, no-cache");
            myHttpWebRequest.KeepAlive = true;
            myHttpWebRequest.AllowAutoRedirect = true;
            using (HttpWebResponse req = (HttpWebResponse)myHttpWebRequest.GetResponse())
            {
                if (req.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = req.GetResponseStream())
                    {
                        if (stream != null)
                        {
                            StreamReader sr = new StreamReader(stream, true);
                            Otvet = sr.ReadToEnd();
                            //Sputnik
                            if (Otvet.IndexOf("fias_id") > 0 && Otvet.IndexOf("coordinates") > 0)
                            {
                                Otvet = Otvet.Substring(Otvet.IndexOf("\"coordinates\":[") + 15);
                                Otvet = Otvet.Remove(Otvet.IndexOf(']'));
                                Coord2 = Otvet.Remove(Otvet.IndexOf(","));
                                Coord1 = Otvet.Substring(Otvet.IndexOf(',') + 1);
                                Result = true;
                            }
                            else
                            {
                                ErrorText = "Координат нет";
                            }
                            sr.Close();
                        }
                        else
                        {
                            ErrorText = "Ответ пуст";
                        }
                        stream.Close();
                    }
                }
                else
                {
                    ErrorText = "Ошибка HTTP:" + ((int)req.StatusCode).ToString() + " " + req.StatusDescription;
                }
                req.Close();
            }
        }
        catch (WebException err)
        {
            if (err.Status == WebExceptionStatus.ProtocolError)
            {
                ErrorText = "Ошибка сети: Ошибка протокола HTTP ";
                ; using (Stream stream = err.Response.GetResponseStream())
                {
                    if (stream != null)
                    {
                        StreamReader sr = new StreamReader(stream, true);
                        ErrorText = ErrorText + sr.ReadToEnd();
                        sr.Close();
                    }
                }
            }
            if (err.Status == WebExceptionStatus.CacheEntryNotFound) { ErrorText = "Ошибка сети: Указанная запись не была найдена в кэше"; }
            if (err.Status == WebExceptionStatus.ConnectFailure) { ErrorText = "Ошибка сети: С точкой удаленной службы нельзя связаться на транспортном уровне"; }
            if (err.Status == WebExceptionStatus.ConnectionClosed) { ErrorText = "Ошибка сети: Подключение было преждевреммно закрыто"; }
            if (err.Status == WebExceptionStatus.KeepAliveFailure) { ErrorText = "Ошибка сети: Неожиданно закрыто подключение для запроса, задающего загаловок Keep-alive"; }
            if (err.Status == WebExceptionStatus.MessageLengthLimitExceeded) { ErrorText = "Ошибка сети: Принято сообщение о превышении заданного ограничения при передаче запроса или приеме ответа сервера"; }
            if (err.Status == WebExceptionStatus.NameResolutionFailure) { ErrorText = "Ошибка сети: Служба разрешения имен не можеть разрешить имя узла"; }
            if (err.Status == WebExceptionStatus.Pending) { ErrorText = "Ошибка сети: Внутренний асинхронный запрос находиться в очереди"; }
            if (err.Status == WebExceptionStatus.PipelineFailure) { ErrorText = "Ошибка сети: Запрос являлся конвеерным запросом и подключение было закрыто до получения запроса"; }
            if (err.Status == WebExceptionStatus.ProxyNameResolutionFailure) { ErrorText = "Ошибка сети: Служба разрешения имен не может распознать имя узла проски сервера"; }
            if (err.Status == WebExceptionStatus.ReceiveFailure) { ErrorText = "Ошибка сети: От удаленного сервера не был получен полный ответ"; }
            if (err.Status == WebExceptionStatus.RequestCanceled) { ErrorText = "Ошибка сети: Запрос был отменен"; }
            if (err.Status == WebExceptionStatus.RequestProhibitedByCachePolicy) { ErrorText = "Ошибка сети: Запрос не разрешен политикой кеширования"; }
            if (err.Status == WebExceptionStatus.RequestProhibitedByProxy) { ErrorText = "Ошибка сети: Этот запрос не разрешен прокси сервером"; }
            if (err.Status == WebExceptionStatus.SendFailure) { ErrorText = "Ошибка сети: Полный запрос не был передан на удаленный сервер"; }
            if (err.Status == WebExceptionStatus.ServerProtocolViolation) { ErrorText = "Ошибка сети: Ответ сервера не являлся допустимым ответом HTTP"; }
            if (err.Status == WebExceptionStatus.Timeout) { ErrorText = "Ошибка сети: В течение времени времени ожидания ответ получен не был"; }
            if (err.Status == WebExceptionStatus.TrustFailure) { ErrorText = "Ошибка сети: Сертификат сервера не может быть проверен"; }
            if (err.Status == WebExceptionStatus.UnknownError) { ErrorText = "Ошибка сети: Возникло исключение не известного типа"; }
        }
        catch (Exception err)
        {
            ErrorText = err.Message;
        }
        return Result;
    }

}