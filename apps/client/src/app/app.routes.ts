import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { FilmsComponent } from './pages/films/films.component';
import { FilmComponent } from './pages/film/film.component';
import { PersonComponent } from './pages/person/person.component';
import { AboutComponent } from './pages/about/about.component';
import { AuthRouteComponent } from './pages/auth-route/auth-route.component';
import { CreateFilmComponent } from './pages/create-film/create-film.component';
import { CreateSourceComponent } from './pages/create-source/create-source.component';
import { adminGuard, authGuard, sysAdminGuard } from './guards/auth.guard';
import { UsersComponent } from './pages/users/users.component';
import { BulkSyncComponent } from './pages/bulk-sync/bulk-sync.component';

export const routes: Routes = [
    {
        path: "",
        component: HomeComponent
    },
    {
        path: "films",
        component: FilmsComponent
    },
    {
        path: "film/:id",
        component: FilmComponent
    },
    {
        path: "person/:id",
        component: PersonComponent
    },
    {
        path: "about",
        component: AboutComponent
    },
    // Auth has no pages of its own any more - each of these opens the matching
    // form in tfa-auth-modal over the home page. They stay as routes because
    // the registration, reset and welcome emails all link to them.
    {
        path: "requestAccount",
        component: AuthRouteComponent,
        data: { authMode: 'register' }
    },
    {
        path: "register/:token",
        component: AuthRouteComponent,
        data: { authMode: 'register' }
    },
    {
        path: "login",
        component: AuthRouteComponent,
        data: { authMode: 'login' }
    },
    {
        path: "forgotPassword",
        component: AuthRouteComponent,
        data: { authMode: 'forgotPassword' }
    },
    {
        path: "resetPassword/:token",
        component: AuthRouteComponent,
        data: { authMode: 'resetPassword' }
    },
    {
        path: "createFilm",
        component: CreateFilmComponent,
        canActivate: [adminGuard]
    },
    {
        path: "createSource/:filmId",
        component: CreateSourceComponent,
        canActivate: [adminGuard]
    },
    {
        path: "users",
        component: UsersComponent,
        canActivate: [sysAdminGuard]
    },
    {
        path: "admin/bulkSync",
        component: BulkSyncComponent,
        canActivate: [sysAdminGuard]
    }

];
