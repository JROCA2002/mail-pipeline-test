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
        descriptionVision: `Buscamos ser un actor clave en la adquisición de productos vitales en la región
        y en el mundo, a partir de la calidad de nuestros productos y visión estratégica.`,

        descriptionWhatWeDo: "Ofrecemos commodities con un servicio integral en todo el mundo.",

        products: "Nuestros Productos",
        descriptionProducts: "Nuestro portafolio incluye soluciones actualmente disponibles y líneas en desarrollo.",
        metals: "Metales",
        descriptionMetals: "Amplia oferta de metales no ferrosos para abastecer los diferentes segmentos del mercado.",
        aluminio: "Aluminio",
        cobre: "Cobre",
        silicio: "Silicio",
        magnesio: "Magnesio",
        zinc: "Zinc",

        descritionOil: "Productos refinados y servicios logísticos.",
        renovables: "Renovables",
        descriptionRenovables: "Comercialización de diferentes energías renovables.",
        reciclado: "Reciclado",
        descriptionReciclado: "Productos reciclados, principalmente en el mercado de aluminio.",

        services: "Nuestros Servicios",
        financiacion: "Financiación",
        descriptionFinanciacion: "Buscamos ofrecer soluciones financieras para nuestros clientes.",
        logistica: "Logística",
        descriptionLogistica: "Ofrecemos servicios logísticos intermodales en diferentes países del mundo.",
        almacenamiento: "Almacenamiento",
        descriptionWarehousing: "Trabajamos con depósitos globales para optimizar nuestras entregas.",

        titleContact: "Envíanos tu consulta",
        descriptionContact: "Contáctanos para más información.",

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

        description: `At Electa Trading, we efficiently and reliably supply customers worldwide with
        aluminum and other commodities. As a company controlled by Aluar Aluminio
        Argentino, we provide logistics services, financing, and risk management
        solutions since 2022 to ensure our clients operate with predictability, agility, and
        support.`,
        location: "Location",
        team: "Our team",
        mision: "Mission",
        descriptionMision: `We provide comprehensive solutions to our clients, adding value to the raw
        materials supply chain. Our approach is driven by professional teams with a
        strong understanding of the markets where we operate.`,
        vision: "Vision",
        descriptionVision: `To become a key player in the acquisition of vital products in the region and the
        world, grounded on our strategic vision and the quality of our products.`,
        descriptionWhatWeDo: "We commercialize commodities with comprehensive service worldwide.",

        products: "Our Products",
        descriptionProducts: "Our portfolio features currently available solutions, as well as lines under development.",
        metals: "Metals",
        descriptionMetals: "A wide range of non-ferrous metals to supply different market segments.",
        aluminio: "Aluminum",
        cobre: "Copper",
        silicio: "Silicon",
        magnesio: "Magnesium",
        zinc: "Zinc",

        descriptionOil: "Refined products and logistics services.",
        renovables: "Renowables",
        descriptionRenovables: "Marketing of various renewable energy sources.",
        reciclado: "Recycling",
        descriptionReciclado: "Recycled products, primarily in the aluminum market.",

        services: "Our Services",
        financiacion: "Financing",
        descriptionFinanciacion: "We offer financial solutions for our clients.",
        logistica: "Logistic",
        descriptionLogistica: "We provide intermodal logistics services in various countries worldwide.",
        almacenamiento: "Warehousing",
        descriptionWarehousing: "We work with warehouses globally to optimize our deliveries.",

        titleContact: "Send us your inquiry",
        descriptionContact: "Contact us for more information.",

        labelName: "Name (*)",
        labelCompany: "Company (*)",
        labelTelephone: "Phone (*)",
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


$(".lang-btn").click(function () {
    const selectedLang = $(this).data("lang");
    setLanguage(selectedLang);
   
});

/* Al cargar la página, usar idioma guardado
$(document).ready(function () {
    const savedLang = localStorage.getItem("lang") || "es";
    setLanguage(savedLang);
});
*/