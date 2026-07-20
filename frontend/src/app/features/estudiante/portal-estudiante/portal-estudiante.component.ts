import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import jsPDF from 'jspdf';
import { EstudiantesService } from '../../../core/services/estudiantes.service';
import { TIPO_APOYO_LABEL, ESTADO_LABEL } from '../../../core/models/enums';
import { SolicitudListItem } from '../../../core/models/solicitud.models';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';
import {
  CRITERIOS_FILTRO_SOLICITUDES_INICIALES,
  CriteriosFiltroSolicitudes,
  FiltrosSolicitudesComponent,
} from '../../../shared/components/filtros-solicitudes/filtros-solicitudes.component';

@Component({
  selector: 'app-portal-estudiante',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CurrencyPipe,
    DatePipe,
    RouterLink,
    StatusBadgeComponent,
    SpinnerComponent,
    FiltrosSolicitudesComponent,
  ],
  templateUrl: './portal-estudiante.component.html',
  styleUrl: './portal-estudiante.component.scss',
})
export class PortalEstudianteComponent {
  private readonly estudiantesService = inject(EstudiantesService);

  readonly tipoLabel = TIPO_APOYO_LABEL;
  readonly estadoLabel = ESTADO_LABEL;
  readonly cargando = signal(true);
  readonly error = signal<string | null>(null);
  readonly solicitudes = signal<SolicitudListItem[]>([]);

  // El listado ya se trae completo (hasta 50) en una sola petición, así que
  // el filtrado es local — no hace falta volver a golpear el servidor.
  private readonly criterios = signal<CriteriosFiltroSolicitudes>({
    ...CRITERIOS_FILTRO_SOLICITUDES_INICIALES,
  });

  readonly solicitudesFiltradas = computed(() => {
    const { estado, tipoApoyo, fechaDesde, fechaHasta } = this.criterios();

    return this.solicitudes().filter((s) => {
      const coincideEstado = estado === null || s.estado === estado;
      const coincideTipo = tipoApoyo === null || s.tipoApoyo === tipoApoyo;
      const coincideRango = this.estaEnRangoDeFechas(s.fechaSolicitud, fechaDesde, fechaHasta);
      return coincideEstado && coincideTipo && coincideRango;
    });
  });

  constructor() {
    this.estudiantesService.obtenerPropio().subscribe({
      next: (estudiante) => this.cargarSolicitudes(estudiante.id),
      error: () => {
        this.error.set('No fue posible cargar su información de estudiante.');
        this.cargando.set(false);
      },
    });
  }

  private cargarSolicitudes(estudianteId: string): void {
    this.estudiantesService
      .listarSolicitudes(estudianteId, 1, 50)
      .pipe(finalize(() => this.cargando.set(false)))
      .subscribe((resultado) => this.solicitudes.set(resultado.items));
  }

  onAplicarFiltros(criterios: CriteriosFiltroSolicitudes): void {
    this.criterios.set(criterios);
  }

  /** Compara solo la parte de fecha (ignora la hora) para que el filtro
   *  "Hasta" incluya el día completo seleccionado. */
  private estaEnRangoDeFechas(
    fechaSolicitud: string,
    fechaDesde: string | null,
    fechaHasta: string | null
  ): boolean {
    const fecha = fechaSolicitud.slice(0, 10);
    if (fechaDesde && fecha < fechaDesde) return false;
    if (fechaHasta && fecha > fechaHasta) return false;
    return true;
  }

  /** Constancia en PDF generada en el cliente para RF-04 (sin requerir backend adicional). */
  descargarConstancia(solicitud: SolicitudListItem): void {
    const doc = new jsPDF({ unit: 'mm', format: 'a4' });
    const pageWidth = doc.internal.pageSize.getWidth();
    const marginX = 20;
    let y = 20;

    const azulOscuro: [number, number, number] = [17, 34, 64];
    const dorado: [number, number, number] = [193, 143, 79];
    const gris: [number, number, number] = [110, 110, 110];

    // --- Encabezado ---
    doc.setFillColor(...azulOscuro);
    doc.rect(0, 0, pageWidth, 32, 'F');

    doc.setTextColor(255, 255, 255);
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(18);
    doc.text('EduApoyos', marginX, 15);

    doc.setFont('helvetica', 'normal');
    doc.setFontSize(10);
    doc.text('Apoyos económicos', marginX, 22);

    doc.setFontSize(10);
    doc.setTextColor(...dorado);
    doc.text('CONSTANCIA DE SOLICITUD', pageWidth - marginX, 18, { align: 'right' });

    y = 45;

    // --- Título ---
    doc.setTextColor(...azulOscuro);
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(14);
    doc.text(this.tipoLabel[solicitud.tipoApoyo], marginX, y);

    doc.setFont('helvetica', 'normal');
    doc.setFontSize(11);
    doc.setTextColor(...gris);
    doc.text(`ID de solicitud: ${solicitud.id}`, marginX, y + 7);

    y += 20;

    // --- Línea divisoria ---
    doc.setDrawColor(...dorado);
    doc.setLineWidth(0.5);
    doc.line(marginX, y, pageWidth - marginX, y);
    y += 12;

    // --- Tabla de datos ---
    const filas: [string, string][] = [
      [
        'Monto solicitado',
        new Intl.NumberFormat('es-CO', {
          style: 'currency',
          currency: 'COP',
          maximumFractionDigits: 0,
        }).format(solicitud.montoSolicitado),
      ],
      ['Estado actual', this.estadoLabel[solicitud.estado]],
      [
        'Fecha de solicitud',
        new Date(solicitud.fechaSolicitud).toLocaleDateString('es-CO', {
          day: '2-digit',
          month: 'long',
          year: 'numeric',
        }),
      ],
      [
        'Última actualización',
        new Date(solicitud.fechaActualizacion).toLocaleDateString('es-CO', {
          day: '2-digit',
          month: 'long',
          year: 'numeric',
        }),
      ],
    ];

    const colLabelWidth = 55;

    filas.forEach(([label, valor]) => {
      doc.setFont('helvetica', 'bold');
      doc.setFontSize(10);
      doc.setTextColor(...azulOscuro);
      doc.text(label, marginX, y);

      doc.setFont('helvetica', 'normal');
      doc.setTextColor(60, 60, 60);
      doc.text(valor, marginX + colLabelWidth, y);

      y += 9;
      doc.setDrawColor(230, 230, 230);
      doc.setLineWidth(0.2);
      doc.line(marginX, y - 4, pageWidth - marginX, y - 4);
    });

    y += 10;

    // --- Nota ---
    doc.setFillColor(245, 245, 245);
    doc.roundedRect(marginX, y, pageWidth - marginX * 2, 20, 2, 2, 'F');
    doc.setFont('helvetica', 'italic');
    doc.setFontSize(9);
    doc.setTextColor(...gris);
    doc.text(
      'Este documento certifica el registro de la solicitud en la plataforma EduApoyos.',
      pageWidth / 2,
      y + 8,
      { align: 'center', maxWidth: pageWidth - marginX * 2 - 10 },
    );
    doc.text('No requiere firma para su validez interna.', pageWidth / 2, y + 14, {
      align: 'center',
    });

    // --- Pie de página ---
    const pageHeight = doc.internal.pageSize.getHeight();
    doc.setFontSize(8);
    doc.setTextColor(...gris);
    doc.text(
      `Documento generado automáticamente el ${new Date().toLocaleDateString('es-CO')} por EduApoyos.`,
      pageWidth / 2,
      pageHeight - 12,
      { align: 'center' },
    );

    doc.save(`constancia-${solicitud.id.slice(0, 8)}.pdf`);
  }
}
