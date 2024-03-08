using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citynames;

public delegate ActionResult LoggableActionDelegate();
public class LoggableAction(LoggableActionDelegate @delegate)
{
    public ActionResult Invoke() => @delegate();
    public static implicit operator LoggableAction(LoggableActionDelegate @delegate)
        => new(@delegate);
    public static implicit operator LoggableActionDelegate(LoggableAction la)
        => la.Invoke;
}
