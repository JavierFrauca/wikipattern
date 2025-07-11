using System;
using System.Collections.Generic;
using System.Text;

// ============================================================================
// BUILDER PATTERN - IMPLEMENTACIÓN DIDÁCTICA
// Ejemplo: Constructor de PCs personalizadas para una tienda de tecnología
// ============================================================================

// ============================================================================
// PRODUCTO COMPLEJO QUE VAMOS A CONSTRUIR
// ============================================================================
public class Computer
{
    public string Processor { get; set; }
    public string MotherBoard { get; set; }
    public string RAM { get; set; }
    public string Storage { get; set; }
    public string GraphicsCard { get; set; }
    public string PowerSupply { get; set; }
    public string Case { get; set; }
    public List<string> Peripherals { get; set; } = new List<string>();
    public decimal TotalPrice { get; set; }
    public string ConfigurationType { get; set; }

    public void ShowConfiguration()
    {
        Console.WriteLine($"\n🖥️ CONFIGURACIÓN DE PC - {ConfigurationType}");
        Console.WriteLine("=" + new string('=', ConfigurationType.Length + 25));
        Console.WriteLine($"   💾 Procesador: {Processor}");
        Console.WriteLine($"   🔌 Tarjeta Madre: {MotherBoard}");
        Console.WriteLine($"   🧠 Memoria RAM: {RAM}");
        Console.WriteLine($"   💽 Almacenamiento: {Storage}");
        Console.WriteLine($"   🎮 Tarjeta Gráfica: {GraphicsCard}");
        Console.WriteLine($"   🔋 Fuente de Poder: {PowerSupply}");
        Console.WriteLine($"   📦 Gabinete: {Case}");
        
        if (Peripherals.Any())
        {
            Console.WriteLine($"   🖱️ Periféricos: {string.Join(", ", Peripherals)}");
        }
        
        Console.WriteLine($"   💰 Precio Total: ${TotalPrice:F2}");
    }

    public bool IsComplete()
    {
        return !string.IsNullOrEmpty(Processor) &&
               !string.IsNullOrEmpty(MotherBoard) &&
               !string.IsNullOrEmpty(RAM) &&
               !string.IsNullOrEmpty(Storage) &&
               !string.IsNullOrEmpty(GraphicsCard) &&
               !string.IsNullOrEmpty(PowerSupply) &&
               !string.IsNullOrEmpty(Case);
    }
}

// ============================================================================
// BUILDER ABSTRACTO - DEFINE LOS PASOS DE CONSTRUCCIÓN
// ============================================================================
public abstract class ComputerBuilder
{
    protected Computer _computer = new Computer();

    // Métodos abstractos que deben implementar los builders concretos
    public abstract ComputerBuilder BuildProcessor();
    public abstract ComputerBuilder BuildMotherBoard();
    public abstract ComputerBuilder BuildRAM();
    public abstract ComputerBuilder BuildStorage();
    public abstract ComputerBuilder BuildGraphicsCard();
    public abstract ComputerBuilder BuildPowerSupply();
    public abstract ComputerBuilder BuildCase();
    public abstract ComputerBuilder AddPeripherals();

    // Método común para obtener el resultado
    public Computer GetComputer()
    {
        if (!_computer.IsComplete())
        {
            throw new InvalidOperationException("La PC no está completamente configurada");
        }
        return _computer;
    }

    // Reset para reutilizar el builder
    public virtual ComputerBuilder Reset()
    {
        _computer = new Computer();
        return this;
    }
}

// ============================================================================
// BUILDERS CONCRETOS - DIFERENTES CONFIGURACIONES
// ============================================================================

// Builder para PC Gaming de alta gama
public class GamingPCBuilder : ComputerBuilder
{
    public GamingPCBuilder()
    {
        _computer.ConfigurationType = "PC Gaming de Alta Gama";
    }

    public override ComputerBuilder BuildProcessor()
    {
        Console.WriteLine("🔧 Instalando procesador gaming de última generación...");
        _computer.Processor = "Intel Core i9-13900K (24 cores, 5.8GHz)";
        _computer.TotalPrice += 589.99m;
        return this;
    }

    public override ComputerBuilder BuildMotherBoard()
    {
        Console.WriteLine("🔧 Instalando tarjeta madre para gaming...");
        _computer.MotherBoard = "ASUS ROG Strix Z790-E Gaming (WiFi 6E, DDR5)";
        _computer.TotalPrice += 389.99m;
        return this;
    }

    public override ComputerBuilder BuildRAM()
    {
        Console.WriteLine("🔧 Instalando memoria RAM de alto rendimiento...");
        _computer.RAM = "Corsair Vengeance DDR5-5600 32GB (2x16GB)";
        _computer.TotalPrice += 249.99m;
        return this;
    }

    public override ComputerBuilder BuildStorage()
    {
        Console.WriteLine("🔧 Instalando almacenamiento ultrarrápido...");
        _computer.Storage = "Samsung 980 PRO NVMe SSD 2TB + WD Black HDD 4TB";
        _computer.TotalPrice += 449.99m;
        return this;
    }

    public override ComputerBuilder BuildGraphicsCard()
    {
        Console.WriteLine("🔧 Instalando tarjeta gráfica de última generación...");
        _computer.GraphicsCard = "NVIDIA GeForce RTX 4080 SUPER 16GB";
        _computer.TotalPrice += 999.99m;
        return this;
    }

    public override ComputerBuilder BuildPowerSupply()
    {
        Console.WriteLine("🔧 Instalando fuente de poder modular...");
        _computer.PowerSupply = "Corsair RM850x 850W 80+ Gold Modular";
        _computer.TotalPrice += 149.99m;
        return this;
    }

    public override ComputerBuilder BuildCase()
    {
        Console.WriteLine("🔧 Instalando gabinete gaming con RGB...");
        _computer.Case = "Corsair iCUE 4000X RGB Tempered Glass";
        _computer.TotalPrice += 129.99m;
        return this;
    }

    public override ComputerBuilder AddPeripherals()
    {
        Console.WriteLine("🔧 Añadiendo periféricos gaming...");
        _computer.Peripherals.AddRange(new[]
        {
            "Teclado mecánico Razer BlackWidow V3",
            "Mouse Logitech G Pro X Superlight",
            "Monitor ASUS ROG Swift PG279QM 27\" 240Hz",
            "Headset SteelSeries Arctis Pro Wireless"
        });
        _computer.TotalPrice += 899.99m;
        return this;
    }
}

// Builder para PC de Oficina económico
public class OfficePCBuilder : ComputerBuilder
{
    public OfficePCBuilder()
    {
        _computer.ConfigurationType = "PC de Oficina Económico";
    }

    public override ComputerBuilder BuildProcessor()
    {
        Console.WriteLine("🔧 Instalando procesador eficiente para oficina...");
        _computer.Processor = "AMD Ryzen 5 5600G (6 cores, 4.4GHz, gráficos integrados)";
        _computer.TotalPrice += 159.99m;
        return this;
    }

    public override ComputerBuilder BuildMotherBoard()
    {
        Console.WriteLine("🔧 Instalando tarjeta madre básica...");
        _computer.MotherBoard = "MSI B450M PRO-VDH MAX (Micro-ATX)";
        _computer.TotalPrice += 79.99m;
        return this;
    }

    public override ComputerBuilder BuildRAM()
    {
        Console.WriteLine("🔧 Instalando memoria RAM estándar...");
        _computer.RAM = "Corsair Vengeance LPX DDR4-3200 16GB (2x8GB)";
        _computer.TotalPrice += 89.99m;
        return this;
    }

    public override ComputerBuilder BuildStorage()
    {
        Console.WriteLine("🔧 Instalando almacenamiento SSD básico...");
        _computer.Storage = "Kingston NV2 NVMe SSD 500GB";
        _computer.TotalPrice += 39.99m;
        return this;
    }

    public override ComputerBuilder BuildGraphicsCard()
    {
        Console.WriteLine("🔧 Usando gráficos integrados (ahorro de costos)...");
        _computer.GraphicsCard = "AMD Radeon Vega 7 (Integrados en CPU)";
        _computer.TotalPrice += 0m; // Ya incluido en el procesador
        return this;
    }

    public override ComputerBuilder BuildPowerSupply()
    {
        Console.WriteLine("🔧 Instalando fuente de poder eficiente...");
        _computer.PowerSupply = "EVGA BR 450W 80+ Bronze";
        _computer.TotalPrice += 49.99m;
        return this;
    }

    public override ComputerBuilder BuildCase()
    {
        Console.WriteLine("🔧 Instalando gabinete compacto...");
        _computer.Case = "Fractal Design Core 1000 Micro-ATX";
        _computer.TotalPrice += 39.99m;
        return this;
    }

    public override ComputerBuilder AddPeripherals()
    {
        Console.WriteLine("🔧 Añadiendo periféricos básicos de oficina...");
        _computer.Peripherals.AddRange(new[]
        {
            "Teclado Logitech K120",
            "Mouse Logitech B100",
            "Monitor ASUS VA24EHE 24\" 1080p"
        });
        _computer.TotalPrice += 199.99m;
        return this;
    }
}

// Builder para Workstation profesional
public class WorkstationBuilder : ComputerBuilder
{
    public WorkstationBuilder()
    {
        _computer.ConfigurationType = "Workstation Profesional";
    }

    public override ComputerBuilder BuildProcessor()
    {
        Console.WriteLine("🔧 Instalando procesador profesional multi-core...");
        _computer.Processor = "AMD Ryzen Threadripper PRO 5975WX (32 cores, 4.5GHz)";
        _computer.TotalPrice += 2599.99m;
        return this;
    }

    public override ComputerBuilder BuildMotherBoard()
    {
        Console.WriteLine("🔧 Instalando tarjeta madre workstation...");
        _computer.MotherBoard = "ASUS Pro WS WRX80E-SAGE SE WIFI (sWRX8)";
        _computer.TotalPrice += 899.99m;
        return this;
    }

    public override ComputerBuilder BuildRAM()
    {
        Console.WriteLine("🔧 Instalando memoria RAM ECC profesional...");
        _computer.RAM = "Kingston Server Premier DDR4-3200 ECC 128GB (4x32GB)";
        _computer.TotalPrice += 1599.99m;
        return this;
    }

    public override ComputerBuilder BuildStorage()
    {
        Console.WriteLine("🔧 Instalando almacenamiento enterprise...");
        _computer.Storage = "Samsung PM9A3 NVMe SSD 4TB + WD Gold HDD 8TB RAID";
        _computer.TotalPrice += 1299.99m;
        return this;
    }

    public override ComputerBuilder BuildGraphicsCard()
    {
        Console.WriteLine("🔧 Instalando tarjeta gráfica profesional...");
        _computer.GraphicsCard = "NVIDIA RTX A6000 48GB (CAD/Rendering)";
        _computer.TotalPrice += 4999.99m;
        return this;
    }

    public override ComputerBuilder BuildPowerSupply()
    {
        Console.WriteLine("🔧 Instalando fuente de poder industrial...");
        _computer.PowerSupply = "Corsair AX1600i 1600W 80+ Titanium";
        _computer.TotalPrice += 599.99m;
        return this;
    }

    public override ComputerBuilder BuildCase()
    {
        Console.WriteLine("🔧 Instalando gabinete para workstation...");
        _computer.Case = "Fractal Design Define 7 XL Full Tower";
        _computer.TotalPrice += 199.99m;
        return this;
    }

    public override ComputerBuilder AddPeripherals()
    {
        Console.WriteLine("🔧 Añadiendo periféricos profesionales...");
        _computer.Peripherals.AddRange(new[]
        {
            "Teclado mecánico Das Keyboard 4 Professional",
            "Mouse Logitech MX Master 3S",
            "Monitor Dell UltraSharp U3223QE 32\" 4K",
            "Wacom Cintiq Pro 24\" (para diseño)"
        });
        _computer.TotalPrice += 2199.99m;
        return this;
    }
}

// ============================================================================
// DIRECTOR - ORQUESTA EL PROCESO DE CONSTRUCCIÓN
// ============================================================================
public class ComputerAssembler
{
    // Construcción completa paso a paso
    public Computer BuildCompletePC(ComputerBuilder builder)
    {
        Console.WriteLine($"\n🏗️ INICIANDO ENSAMBLAJE DE PC...");
        Console.WriteLine("================================");

        return builder
            .BuildProcessor()
            .BuildMotherBoard()
            .BuildRAM()
            .BuildStorage()
            .BuildGraphicsCard()
            .BuildPowerSupply()
            .BuildCase()
            .AddPeripherals()
            .GetComputer();
    }

    // Construcción básica sin periféricos
    public Computer BuildBasicPC(ComputerBuilder builder)
    {
        Console.WriteLine($"\n🏗️ ENSAMBLAJE BÁSICO DE PC (sin periféricos)...");
        Console.WriteLine("===============================================");

        return builder
            .BuildProcessor()
            .BuildMotherBoard()
            .BuildRAM()
            .BuildStorage()
            .BuildGraphicsCard()
            .BuildPowerSupply()
            .BuildCase()
            .GetComputer();
    }

    // Construcción personalizada
    public Computer BuildCustomPC(ComputerBuilder builder, params Action<ComputerBuilder>[] customSteps)
    {
        Console.WriteLine($"\n🏗️ ENSAMBLAJE PERSONALIZADO...");
        Console.WriteLine("==============================");

        foreach (var step in customSteps)
        {
            step(builder);
        }

        return builder.GetComputer();
    }
}

// ============================================================================
// DEMO DIDÁCTICO
// ============================================================================
public static class BuilderPatternDemo
{
    public static void RunDemo()
    {
        Console.WriteLine("🏭 DEMO: BUILDER PATTERN - ENSAMBLAJE DE PCs PERSONALIZADAS");
        Console.WriteLine("============================================================");

        var assembler = new ComputerAssembler();

        Console.WriteLine("\n🎮 ESCENARIO 1: PC Gaming de Alta Gama");
        Console.WriteLine("--------------------------------------");
        var gamingBuilder = new GamingPCBuilder();
        var gamingPC = assembler.BuildCompletePC(gamingBuilder);
        gamingPC.ShowConfiguration();

        Console.WriteLine("\n\n🏢 ESCENARIO 2: PC de Oficina Económico");
        Console.WriteLine("---------------------------------------");
        var officeBuilder = new OfficePCBuilder();
        var officePC = assembler.BuildCompletePC(officeBuilder);
        officePC.ShowConfiguration();

        Console.WriteLine("\n\n💼 ESCENARIO 3: Workstation Profesional");
        Console.WriteLine("---------------------------------------");
        var workstationBuilder = new WorkstationBuilder();
        var workstationPC = assembler.BuildCompletePC(workstationBuilder);
        workstationPC.ShowConfiguration();

        Console.WriteLine("\n\n🔧 ESCENARIO 4: Construcción Personalizada");
        Console.WriteLine("------------------------------------------");
        var customBuilder = new GamingPCBuilder().Reset();
        var customPC = assembler.BuildCustomPC(customBuilder,
            b => b.BuildProcessor(),
            b => b.BuildMotherBoard(),
            b => b.BuildRAM(),
            b => b.BuildStorage(),
            b => b.BuildGraphicsCard(),
            b => b.BuildPowerSupply(),
            b => b.BuildCase()
            // No incluimos periféricos en esta configuración personalizada
        );
        customPC.ShowConfiguration();

        Console.WriteLine("\n\n📊 COMPARACIÓN DE PRECIOS");
        Console.WriteLine("========================");
        Console.WriteLine($"🎮 PC Gaming: ${gamingPC.TotalPrice:F2}");
        Console.WriteLine($"🏢 PC Oficina: ${officePC.TotalPrice:F2}");
        Console.WriteLine($"💼 Workstation: ${workstationPC.TotalPrice:F2}");
        Console.WriteLine($"🔧 PC Personalizado: ${customPC.TotalPrice:F2}");

        var savings = gamingPC.TotalPrice - customPC.TotalPrice;
        Console.WriteLine($"💰 Ahorro al no incluir periféricos: ${savings:F2}");

        Console.WriteLine("\n✅ Demo del Builder Pattern completado");
        Console.WriteLine("\n💡 LECCIONES APRENDIDAS:");
        Console.WriteLine("   • El Builder permite construir objetos complejos paso a paso");
        Console.WriteLine("   • Diferentes builders pueden crear variaciones del mismo producto");
        Console.WriteLine("   • El Director orquesta el proceso de construcción");
        Console.WriteLine("   • Se puede reutilizar el mismo builder para diferentes configuraciones");
    }
}

// Para ejecutar el demo:
// BuilderPatternDemo.RunDemo();
