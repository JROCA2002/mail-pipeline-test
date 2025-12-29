// js/lang.js
const translations = {
    es: {
        navItem1: "Quiénes somos",
        navItem2: "Misión/Visión",
        navItem3: "¿Qué hacemos?",
        navItem4: "Contacto",

        description: `Desde Electa Trading, abastecemos a clientes de todo el mundo con aluminio y otros commodities de 
        forma eficiente y confiable. Como sociedad controlada por Aluar Aluminio Argentino, desde 2022 proveemos 
        servicios logísticos, ofrecemos soluciones de financiamiento y gestión de riesgos para que nuestros clientes 
        operen con previsibilidad, agilidad y respaldo.`,
        location: "Ubicación",
        team: "Nuestro equipo",
        mision: "Misión",
        descriptionMision: `Brindamos soluciones integrales a nuestros clientes, agregando valor a la cadena de 
        suministro de materias primas. Nos destacamos por el profesionalismo de nuestros equipos y el conocimiento de 
        los mercados donde operamos.`,
        vision: "Visión",
        descriptionVision: `Convertirse en la principal referencia para la adquisición de productos vitales en todas las
        regiones del mundo.`,

        descriptionWhatWeDo: "Ofrecemos commodities con un servicio integral en todo el mundo.",

        products: "Nuestros Productos",
        descriptionProducts: "Nuestro portafolio incluye soluciones actualmente disponibles y líneas en desarrollo.",
        metals: "Metales",
        descriptionMetals: "Contamos con una amplia oferta de productos para abastecer los diferentes segmentos del mercado.",
        aluminio: "Aluminio",
        cobre: "Cobre",
        silicio: "Silicio",
        magnesio: "Magnesio",
        zinc: "Zinc",

        descritionOil: "Ofrecemos productos refinados y servicios logísticos.",
        renovables: "Renovables",
        descriptionRenovables: "Comercialización de diferentes energías renovables.",
        reciclado: "Reciclado",
        descriptionReciclado: "Ofrecemos productos reciclados, principalmente en el mercado de aluminio.",

        services: "Nuestros Servicios",
        financiacion: "Financiación",
        descriptionFinanciacion: "Buscamos ofrecer soluciones financieras para nuestros clientes.",
        logistica: "Logística",
        descriptionLogistica: "Ofrecemos servicios logísticos tanto por barco como por camión en los diferentes países del mundo.",
        descriptionWarehousing: "Trabajamos con depósitos en diferentes zonas del mundo para optimizar nuestras entregas.",

        titleContact: "Envíenos su consulta",
        descriptionContact: "Si desea más información, póngase en contacto con nosotro.",

        labelName: "Nombre (*)",
        labelCompany: "Empresa (*)",
        labelTelephone: "Teléfono (*)",
        labelMessage: "Mensaje (*)",
        labelSend: "Enviar",
    },
    en: {
        navItem1: "Who we are",
        navItem2: "Mission/Vision",
        navItem3: "What we do",
        navItem4: "Contact",

        description: `From Electa Trading, we supply customers around the world with aluminum and other commodities
        efficiently and reliably. As a company controlled by Aluar Aluminio Argentino, since 2022 we provide
        logistics services, offer financing solutions and risk management so that our customers can operate
        with predictability, agility and support.`,
        location: "Location",
        team: "Team",
        mision: "Mission",
        descriptionMision: `We provide comprehensive solutions to our customers, adding value to the supply
        chain of raw materials. We stand out for the professionalism of our teams and the knowledge of the
        markets where we operate.`,
        vision: "Vision",
        descriptionVision: `Become the main reference for vital product procurement in all regions of the world.`,
        descriptionWhatWeDo: "We offer commodities with an integral service all around the world.",

        products: "Our Products",
        descriptionProducts: "Our portfolio includes currently available solutions and lines in development.",
        metals: "Metals",
        descriptionMetals: "We have a wide range of products to supply different market segments.",
        aluminio: "Aluminum",
        cobre: "Copper",
        silicio: "Silicon",
        magnesio: "Magnesium",
        zinc: "Zinc",

        descriptionOil: "We offer refined products and logistic services.",
        renovables: "Renowables",
        descriptionRenovables: "Electa offers different types of renewables energies for consumption.",
        reciclado: "Recycling",
        descriptionReciclado: "We offer recycled products, mainly in the aluminum market.",

        services: "Our Services",
        financiacion: "Financing",
        descriptionFinanciacion: "We do an exhaustive study to provide the best financial service to our customers.",
        logistica: "Logistic",
        descriptionLogistica: "Our goal is to meet our customer’s needs through logistic services by ship or truck all around the world.",
        descriptionWarehousing: "We optimize our deliveries working with warehouses in different regions.",

        titleContact: "Send us your inquiry",
        descriptionContact: "If you would like more information, please contact us.",

        labelName: "Name (*)",
        labelCompany: "Company (*)",
        labelTelephone: "Telephone (*)",
        labelMessage: "Message (*)",
        labelSend: "Send"
    }
};

// Detectar idioma inicial (por ejemplo, navegador o default español)
let currentLang = 'es';

// Función para cambiar idioma
function setLanguage(lang) {
    currentLang = lang;
    $("[data-translate]").each(function () {
        const key = $(this).data("translate");
        const attr = $(this).data("attr");

        // Si tiene data-attr, cambiamos el atributo indicado
        if (attr) {
            $(this).attr(attr, translations[lang][key]);
        } else {
            $(this).text(translations[lang][key]);
        }

    });
}

// Evento de botones
$(".lang-btn").click(function () {
    const selectedLang = $(this).data("lang");
    setLanguage(selectedLang);
    // localStorage.setItem("lang", selectedLang); // guardar preferencia
});

/* Al cargar la página, usar idioma guardado
$(document).ready(function () {
    const savedLang = localStorage.getItem("lang") || "es";
    setLanguage(savedLang);
});
*/