(function () {
  "use strict";

  /**
   * Easy selector helper function
   */
  const select = (el, all = false) => {
    el = el.trim()
    if (all) {
      return [...document.querySelectorAll(el)]
    } else {
      return document.querySelector(el)
    }
  }

  /**
   * Easy event listener function
   */
  const on = (type, el, listener, all = false) => {
    let selectEl = select(el, all)
    if (selectEl) {
      if (all) {
        selectEl.forEach(e => e.addEventListener(type, listener))
      } else {
        selectEl.addEventListener(type, listener)
      }
    }
  }

  /**
   * Easy on scroll event listener 
   */
  const onscroll = (el, listener) => {
    el.addEventListener('scroll', listener)
  }

  /**
   * Navbar links active state on scroll
   */
  let navbarlinks = select('#navbar .scrollto', true)
  const navbarlinksActive = () => {
    let position = window.scrollY + 100
    navbarlinks.forEach(navbarlink => {
      if (!navbarlink.hash) return
      let section = select(navbarlink.hash)
      if (!section) return
      if (position >= section.offsetTop && position <= (section.offsetTop + section.offsetHeight)) {
        navbarlink.classList.add('active')
      } else {
        navbarlink.classList.remove('active')
      }
    })
  }
  window.addEventListener('load', navbarlinksActive)
  onscroll(document, navbarlinksActive)

  /**
   * Scrolls to an element with header offset
   */
  const scrollto = (el) => {
    let header = select('#header')
    let offset = header.offsetHeight

    if (!header.classList.contains('header-scrolled')) {
      offset -= 20
    }

    let elementPos = select(el).offsetTop
    window.scrollTo({
      top: elementPos - offset,
      behavior: 'smooth'
    })
  }

  /**
   * Toggle .header-scrolled class to #header when page is scrolled
   */
  let selectHeader = select('#header')
  if (selectHeader) {
    const headerScrolled = () => {
      if (window.scrollY > 10) {
        selectHeader.classList.add('header-scrolled')
      } else {
        selectHeader.classList.remove('header-scrolled')
      }
    }
    window.addEventListener('load', headerScrolled)
    onscroll(document, headerScrolled)
  }

  /**
   * Back to top button
   */
  let backtotop = select('.back-to-top')
  if (backtotop) {
    const toggleBacktotop = () => {
      if (window.scrollY > 100) {
        backtotop.classList.add('active')
      } else {
        backtotop.classList.remove('active')
      }
    }
    window.addEventListener('load', toggleBacktotop)
    onscroll(document, toggleBacktotop)
  }

  const navbarCollapse = document.getElementById('navbarNav');
  const header = document.getElementById('header');

  navbarCollapse.addEventListener('shown.bs.collapse', () => {
    header.classList.add('menu-open');
  });

  navbarCollapse.addEventListener('hidden.bs.collapse', (e) => {
    e.preventDefault(); // frenamos el cierre inmediato
    header.classList.remove('menu-open');
    // esperamos que termine la animación
    setTimeout(() => {
      bootstrap.Collapse.getOrCreateInstance(navbarCollapse).hide();
    }, 500); // mismo tiempo que el transition

  });

  // CERRAR AL CLICK EN LINKS (mobile)
  document.querySelectorAll('#navbarNav .nav-link').forEach(link => {
    link.addEventListener('click', () => {
      header.classList.remove('menu-open');

      setTimeout(() => {
        bootstrap.Collapse.getOrCreateInstance(navbarCollapse).hide();
      }, 500);
    });
  });

  on('click', '.back-to-top', function (e) {
    e.preventDefault()
    window.scrollTo({
      top: 0,
      behavior: 'smooth'
    })
  })

  /**
   * Scroll with ofset on page load with hash links in the url
   */
  window.addEventListener('load', () => {
    if (window.location.hash) {
      if (select(window.location.hash)) {
        scrollto(window.location.hash)
      }
    }
  });

})()

const form = document.querySelector(".php-email-form");
const successMessage = document.getElementById('success-message');
const errorMessage = document.getElementById('error-message');

function showTemporaryMessage(element, duration = 5000) {
  [successMessage, errorMessage].forEach(msg => {
    msg.classList.remove('show-message');
    msg.style.display = 'none';
  });

  element.style.display = 'block';
  setTimeout(() => {
    element.classList.add('show-message');
  }, 10);

  setTimeout(() => {
    element.classList.remove('show-message');
    setTimeout(() => {
      element.style.display = 'none';
    }, 500);
  }, duration);
}

if (form) {
  form.addEventListener("submit", function (event) {
    event.preventDefault();

    // Obtengo el token que genera Google al marcar "No soy un robot"
    const captchaToken = grecaptcha.getResponse();

    if (!captchaToken) {
      errorMessage.textContent = "Por favor, completa el reCAPTCHA antes de enviar.";
      showTemporaryMessage(errorMessage);
      return;
    }

    const submitButton = form.querySelector('.btn-primary');
    submitButton.disabled = true;

    const data = {
      Token: captchaToken,
      Nombre: form.nombre.value,
      Empresa: form.empresa.value,
      Email: form.email.value,
      Telefono: form.telefono.value,
      Mensaje: form.mensaje.value
    };

    fetch("https://localhost:44347/API/EnviarMailDesdeLandingPage", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data)
    })
      .then(response => response.json())
      .then(result => {

        if (result.result === 0) {
          successMessage.textContent = "¡Mensaje enviado correctamente! Te contactaremos pronto.";
          showTemporaryMessage(successMessage);
          form.reset();
          grecaptcha.reset();
        } else {
          errorMessage.textContent = "Captcha inválido o error al enviar el correo.";
          showTemporaryMessage(errorMessage);
          grecaptcha.reset();
        }
      })
      .catch(error => {
        console.error("Error al procesar el formulario:");
        errorMessage.textContent = "Ocurrió un error de conexión al enviar el formulario.";
        showTemporaryMessage(errorMessage);
      })
      .finally(() => {
        submitButton.disabled = false;
      });
  });
}
