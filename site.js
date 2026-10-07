
document.addEventListener("DOMContentLoaded", () => {
  document.querySelectorAll("[data-slider]").forEach(initSlider);
  const search = document.getElementById("projectSearch");
  if(search) search.addEventListener("input", filterProjects);
  document.addEventListener("keydown", e => {
    if(e.key === "Escape") document.querySelectorAll(".modal.open").forEach(m => m.classList.remove("open"));
  });
});

function initSlider(slider){
  const slides=[...slider.querySelectorAll(".slide")];
  if(slides.length<2) return;
  let index=0;
  const dots=[...slider.querySelectorAll(".dot")];
  const show=i=>{
    index=(i+slides.length)%slides.length;
    slides.forEach((s,n)=>s.classList.toggle("active",n===index));
    dots.forEach((d,n)=>d.classList.toggle("active",n===index));
  };
  slider._showSlide=show;
  slider.querySelector(".prev")?.addEventListener("click",e=>{e.stopPropagation();show(index-1)});
  slider.querySelector(".next")?.addEventListener("click",e=>{e.stopPropagation();show(index+1)});
  dots.forEach((d,n)=>d.addEventListener("click",e=>{e.stopPropagation();show(n)}));
  if(!slider.closest(".modal")) setInterval(()=>show(index+1),5000);
}
function filterProjects(){
  const q=(document.getElementById("projectSearch")?.value||"").trim().toLocaleLowerCase("tr-TR");
  document.querySelectorAll(".project-card").forEach(card=>{
    const name=(card.dataset.name||"").toLocaleLowerCase("tr-TR");
    card.style.display=name.includes(q)?"":"none";
  });
}
function openModal(id){
  document.getElementById(id)?.classList.add("open");
  document.body.style.overflow="hidden";
}
function closeModal(id){
  document.getElementById(id)?.classList.remove("open");
  document.body.style.overflow="";
}
function goToSlide(img,index){
  const modal=img.closest(".modal-panel");
  const slider=modal?.querySelector("[data-slider]");
  if(slider && slider._showSlide) slider._showSlide(index);
}
