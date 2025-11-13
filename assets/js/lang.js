// js/lang.js
const translations = {
    es: {
        navItem1: "Quiénes somos",
        navItem2: "Misión/Visión",
        navItem3: "¿Qué hacemos?",
        navItem4: "Contacto",

        description: `Electa trading es una sociedad controlada por Aluar, Aluminios Argentinos. La misma fue creada en
      2022 con el objetivo de brindar soluciones a clientes finales de la industria del Aluminio y otros
      commodities. Principalmente ofreciendo servicio, logística y financiación.`,
        location: "Ubicación",
        team: "Nuestro equipo",
        mision: "Misión",
        descriptionMision: `Electa Trading trabaja para agregar valor a la cadena de suministro de materias primas
        físicas. Nos esforzamos por brindar productos de calidad y un servicio excepcional a
        nuestros clientes, ofreciendo soluciones de financiamiento, gestión de riesgos y servicios
        logísticos a lo largo de toda la cadena de suministro.`,
        vision: "Visión",
        descriptionVision: `Convertirse en la principal referencia para la adquisición de productos vitales en todas las
        regiones del mundo.`,

        descriptionWhatWeDo: "Ofrecemos Commodities con un servicio integral en todo el mundo.",

        products: "Nuestros Productos",
        metals: "Metales",
        descriptionMetals: "Contamos con una amplia oferta de productos para abastecer los diferentes segmentos del mercado",
        aluminio: "Aluminio",
        cobre: "Cobre",
        silicio: "Silicio",
        magnesio: "Magnesio",
        zinc: "Zinc",

        descritionOil: "Ofrecemos productos refinados y servicios logísticos.",
        renovables: "Renovables",
        descriptionRenovables: "Comercialización de diferentes energías renovables.",
        reciclado: "Reciclado",
        descriptionReciclado: "Ofrecemos productos reciclados, principalmente en el mercado de Aluminio.",

        services: "Nuestros Servicios",
        financiacion: "Financiación",
        descriptionFinancion: "Buscamos ofrecer soluciones financieras para nuestros clientes",
        logistica: "Logística",
        descriptionLogistica: "Ofrecemos servicios logísticos tanto por barco como por camión en los diferentes países del mundo.",
        descriptionWarehousing: "Trabajamos con depósitos en diferentes zonas del mundo para optimizar nuestras entregas.",

        titleContact: "Envíenos su consulta",
        descriptionContact: "Si desea más información, póngase en contacto con nosotros",

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

        description: `Electa trading is a subsidiary of Aluar Aluminio Argentino. It was created in 2022 with the objective of delivering solutions to the final customers of the Aluminum Industry and other commodities.  Mainly offering services, logistics and financing.`,
        location: "Location",
        team: "Team",
        mision: "Mission",
        descriptionMision: `Electa Trading works to add value to the supply chain of Physical commodities. We strive to provide quality products and maintain an exceptional service for our customers by offering financing solutions, risk management and logistic services throughout the supply chain.`,
        vision: "Vision",
        descriptionVision: `Become the main reference for vital product procurement in all regions of the world.`,
        descriptionWhatWeDo: "We offer Commodities with an integral service all around the world.",

        products: "Our Products",
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
        descriptionReciclado: "We offer recycled products, mainly in the aluminum market",

        services: "Our Services",
        financiacion: "Financing",
        descriptionFinancion: "We do an exhaustive study to provide the best financial service to our customers.",
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